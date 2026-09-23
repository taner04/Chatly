using System.Data;
using System.Data.Common;
using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.WebApi.Features.Calls.Enums;
using Chatly.WebApi.Features.Calls.Models;
using Chatly.WebApi.Features.Calls.Jobs;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using ContractCallEndReason = Chatly.Contracts.Features.Hubs.CallEndReason;
using ContractCallState = Chatly.Contracts.Features.Hubs.CallState;
using ContractCall = Chatly.Contracts.Features.Hubs.Call;
using DomainCall = Chatly.WebApi.Features.Calls.Models.Call;
using DomainCallEndReason = Chatly.WebApi.Features.Calls.Enums.CallEndReason;

namespace Chatly.WebApi.Features.Hubs.CallingHub;

internal sealed class CallingHub(
    ChatlyDbContext context,
    CurrentUserService currentUser,
    IBackgroundJobClient backgroundJobClient,
    ILogger<CallingHub> logger) : HubBase<ICallingHubClient>(context, currentUser), ICallingHubServer
{
    private static readonly TimeSpan RingingTimeout = TimeSpan.FromSeconds(45);
    private const int MaxSdpLength = 65_536;
    private const int MaxIceCandidateLength = 8_192;
    private const int MaxSdpMidLength = 256;

    public async Task<CallInfo> StartCallAsync(Guid calleeUserId)
    {
        if (calleeUserId == Guid.Empty)
        {
            throw new HubException("The callee user ID is invalid.");
        }

        var cancellationToken = Context.ConnectionAborted;
        var callerUserId = await GetCurrentUserIdAsync(cancellationToken);
        var receiverUserId = UserId.From(calleeUserId);
        if (callerUserId == receiverUserId)
        {
            throw new HubException("A user cannot call themselves.");
        }

        try
        {
            var result = await ExecuteMutationAsync(async () =>
            {
                var receiver = await Database.Users.SingleOrDefaultAsync(
                    user => user.Id == receiverUserId,
                    cancellationToken);
                if (receiver is null)
                {
                    throw new HubException("The user was not found.");
                }

                var areFriends = await Database.Friendships
                    .ForUser(callerUserId)
                    .AnyAsync(
                        friendship => friendship.FirstUserId == receiverUserId ||
                                      friendship.SecondUserId == receiverUserId,
                        cancellationToken);
                if (!areFriends)
                {
                    throw new HubException("Calls can only be started with friends.");
                }

                var call = new DomainCall(callerUserId, receiverUserId);
                Database.Calls.Add(call);
                Database.ActiveCallParticipants.AddRange(
                    new ActiveCallParticipant(callerUserId, call.Id),
                    new ActiveCallParticipant(receiverUserId, call.Id));

                return (Call: call, Receiver: receiver);
            });

            var caller = await Database.Users.AsNoTracking()
                .SingleAsync(user => user.Id == callerUserId, cancellationToken);
            TryScheduleExpiry(result.Call.Id);
            await PublishBestEffortAsync(receiverUserId, new IncomingCallNotification(
                result.Call.Id.Value,
                callerUserId.Value,
                caller.Username,
                CallRole.Receiver,
                ContractCallState.Ringing));
            await PublishToSiblingDevicesBestEffortAsync(callerUserId, new CallStateChangedNotification(
                result.Call.Id.Value,
                receiverUserId.Value,
                result.Receiver.Username,
                CallRole.Caller,
                ContractCallState.Ringing));

            return new CallInfo(
                result.Call.Id.Value,
                receiverUserId.Value,
                result.Receiver.Username,
                CallRole.Caller,
                ContractCallState.Ringing,
                null);
        }
        catch (DbUpdateException)
        {
            throw new HubException("One of the users is already in a call.");
        }
        catch (DbException)
        {
            throw new HubException("The call could not be started.");
        }
    }

    public async Task<CallInfo?> GetCurrentCallAsync()
    {
        var cancellationToken = Context.ConnectionAborted;
        var actorUserId = await GetCurrentUserIdAsync(cancellationToken);
        var call = await Database.Calls
            .AsNoTracking()
            .Include(activeCall => activeCall.CallerUser)
            .Include(activeCall => activeCall.ReceiverUser)
            .SingleOrDefaultAsync(
                activeCall => activeCall.ActiveParticipants.Any(participant => participant.UserId == actorUserId),
                cancellationToken);

        return call is null ? null : CreateCallInfo(call, actorUserId);
    }

    public async Task AcceptCallAsync(Guid callId)
    {
        var result = await MutateCallAsync(callId, (call, actor) => call.Accept(actor, DateTimeOffset.UtcNow));
        await PublishBestEffortAsync(result.Call.CallerUserId, new CallAcceptedNotification(
            result.Call.Id.Value,
            result.Call.ReceiverUserId.Value,
            result.Call.ReceiverUser.Username,
            CallRole.Caller,
            ContractCallState.Accepted));
        await PublishToSiblingDevicesBestEffortAsync(result.Call.ReceiverUserId, new CallStateChangedNotification(
            result.Call.Id.Value,
            result.Call.CallerUserId.Value,
            result.Call.CallerUser.Username,
            CallRole.Receiver,
            ContractCallState.Accepted));
    }

    public async Task RejectCallAsync(Guid callId)
    {
        var result = await MutateCallAsync(
            callId,
            (call, actor) => call.Reject(actor, DateTimeOffset.UtcNow),
            removeReservations: true);
        await PublishEndedAsync(result, static (callId, remoteId, remoteName, role, state, reason) =>
            new CallRejectedNotification(callId, remoteId, remoteName, role, state, reason));
    }

    public async Task EndCallAsync(Guid callId)
    {
        var result = await MutateCallAsync(
            callId,
            (call, actor) => call.End(actor, DateTimeOffset.UtcNow),
            removeReservations: true);
        await PublishEndedAsync(result, static (callId, remoteId, remoteName, role, state, reason) =>
            new CallEndedNotification(callId, remoteId, remoteName, role, state, reason));
    }

    public async Task SendOfferAsync(Guid callId, string sdp)
    {
        EnsurePayload(sdp, MaxSdpLength, "offer");
        var result = await MutateCallAsync(callId, (call, actor) => call.Offer(actor));
        await PublishBestEffortAsync(result.RecipientUserId, new WebRtcOfferNotification(
            result.Call.Id.Value,
            result.ActorUserId.Value,
            result.ActorUsername,
            CallRole.Receiver,
            ContractCallState.Offered,
            sdp));
        await PublishToSiblingDevicesBestEffortAsync(result.ActorUserId, new CallStateChangedNotification(
            result.Call.Id.Value,
            result.Call.ReceiverUserId.Value,
            result.Call.ReceiverUser.Username,
            CallRole.Caller,
            ContractCallState.Offered));
    }

    public async Task SendAnswerAsync(Guid callId, string sdp)
    {
        EnsurePayload(sdp, MaxSdpLength, "answer");
        var result = await MutateCallAsync(callId, (call, actor) => call.Answer(actor));
        await PublishBestEffortAsync(result.RecipientUserId, new WebRtcAnswerNotification(
            result.Call.Id.Value,
            result.ActorUserId.Value,
            result.ActorUsername,
            CallRole.Caller,
            ContractCallState.Active,
            sdp));
        await PublishToSiblingDevicesBestEffortAsync(result.ActorUserId, new CallStateChangedNotification(
            result.Call.Id.Value,
            result.Call.CallerUserId.Value,
            result.Call.CallerUser.Username,
            CallRole.Receiver,
            ContractCallState.Active));
    }

    public async Task SendIceCandidateAsync(
        Guid callId,
        string candidate,
        string? sdpMid,
        int? sdpMLineIndex)
    {
        EnsurePayload(candidate, MaxIceCandidateLength, "ICE candidate");
        if (sdpMid?.Length > MaxSdpMidLength)
        {
            throw new HubException("The SDP media ID is too large.");
        }

        if (sdpMLineIndex is < 0 or > ushort.MaxValue)
        {
            throw new HubException("The SDP media line index is invalid.");
        }

        var result = await MutateCallAsync(callId, (call, actor) => call.EnsureCanSendIce(actor));
        var recipientRole = result.RecipientUserId == result.Call.CallerUserId
            ? CallRole.Caller
            : CallRole.Receiver;
        await PublishBestEffortAsync(result.RecipientUserId, new IceCandidateNotification(
            result.Call.Id.Value,
            result.ActorUserId.Value,
            result.ActorUsername,
            recipientRole,
            ToContract(result.Call.Status),
            candidate,
            sdpMid,
            sdpMLineIndex));
    }

    private async Task<(DomainCall Call, UserId ActorUserId, string? ActorUsername, UserId RecipientUserId)>
        MutateCallAsync(Guid callId, Action<DomainCall, UserId> transition, bool removeReservations = false)
    {
        if (callId == Guid.Empty)
        {
            throw new HubException("The call ID is invalid.");
        }

        var cancellationToken = Context.ConnectionAborted;
        var actorUserId = await GetCurrentUserIdAsync(cancellationToken);

        try
        {
            return await ExecuteMutationAsync(async () =>
            {
                var call = await Database.Calls
                    .Include(activeCall => activeCall.CallerUser)
                    .Include(activeCall => activeCall.ReceiverUser)
                    .Include(activeCall => activeCall.ActiveParticipants)
                    .SingleOrDefaultAsync(activeCall => activeCall.Id == CallId.From(callId), cancellationToken);
                if (call is null)
                {
                    throw new HubException("The call was not found.");
                }

                transition(call, actorUserId);
                var recipientUserId = call.GetCounterpart(actorUserId);
                var actor = actorUserId == call.CallerUserId ? call.CallerUser : call.ReceiverUser;
                if (removeReservations)
                {
                    Database.ActiveCallParticipants.RemoveRange(call.ActiveParticipants);
                }

                return (call, actorUserId, actor.Username, recipientUserId);
            });
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new HubException(exception.Message);
        }
        catch (CallTransitionException exception)
        {
            throw new HubException(exception.Message);
        }
        catch (DbUpdateException)
        {
            throw new HubException("The call could not be updated.");
        }
        catch (DbException)
        {
            throw new HubException("The call could not be updated.");
        }
    }

    private async Task<T> ExecuteMutationAsync<T>(Func<Task<T>> operation)
    {
        var strategy = Database.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            Database.ChangeTracker.Clear();
            await using var transaction = await Database.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                Context.ConnectionAborted);
            var result = await operation();
            await Database.SaveChangesAsync(Context.ConnectionAborted);
            await transaction.CommitAsync(Context.ConnectionAborted);
            return result;
        });
    }

    private async Task PublishEndedAsync(
        (DomainCall Call, UserId ActorUserId, string? ActorUsername, UserId RecipientUserId) result,
        Func<Guid, Guid, string?, CallRole, ContractCallState, ContractCallEndReason, ContractCall> factory)
    {
        var reason = ToContract(result.Call.EndReason!.Value);
        await Task.WhenAll(
            PublishBestEffortAsync(result.Call.CallerUserId, factory(
                result.Call.Id.Value,
                result.Call.ReceiverUserId.Value,
                result.Call.ReceiverUser.Username,
                CallRole.Caller,
                ContractCallState.Ended,
                reason)),
            PublishBestEffortAsync(result.Call.ReceiverUserId, factory(
                result.Call.Id.Value,
                result.Call.CallerUserId.Value,
                result.Call.CallerUser.Username,
                CallRole.Receiver,
                ContractCallState.Ended,
                reason)));
    }

    private async Task PublishBestEffortAsync(UserId userId, ContractCall notification)
    {
        try
        {
            await Clients.Group(HubGroups.User(userId)).Receive(notification);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to publish call {CallId} to user {UserId}.", notification.CallId, userId);
        }
    }

    private async Task PublishToSiblingDevicesBestEffortAsync(UserId userId, ContractCall notification)
    {
        try
        {
            await Clients.GroupExcept(HubGroups.User(userId), [Context.ConnectionId]).Receive(notification);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to publish call {CallId} to sibling devices for user {UserId}.",
                notification.CallId,
                userId);
        }
    }

    private void TryScheduleExpiry(CallId callId)
    {
        try
        {
            backgroundJobClient.Schedule<CallExpiryJob>(
                job => job.ExecuteAsync(callId.Value, CancellationToken.None),
                RingingTimeout);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to schedule ringing expiry for call {CallId}.", callId);
        }
    }

    private static CallInfo CreateCallInfo(DomainCall call, UserId actorUserId)
    {
        var remoteUser = actorUserId == call.CallerUserId ? call.ReceiverUser : call.CallerUser;
        return new CallInfo(
            call.Id.Value,
            remoteUser.Id.Value,
            remoteUser.Username,
            actorUserId == call.CallerUserId ? CallRole.Caller : CallRole.Receiver,
            ToContract(call.Status),
            call.EndReason is null ? null : ToContract(call.EndReason.Value));
    }

    private static ContractCallState ToContract(CallStatus status) => status switch
    {
        CallStatus.Ringing => ContractCallState.Ringing,
        CallStatus.Accepted => ContractCallState.Accepted,
        CallStatus.Offered => ContractCallState.Offered,
        CallStatus.Active => ContractCallState.Active,
        CallStatus.Ended => ContractCallState.Ended,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    private static ContractCallEndReason ToContract(DomainCallEndReason reason) => reason switch
    {
        DomainCallEndReason.Completed => ContractCallEndReason.Completed,
        DomainCallEndReason.Declined => ContractCallEndReason.Declined,
        DomainCallEndReason.Cancelled => ContractCallEndReason.Cancelled,
        DomainCallEndReason.Missed => ContractCallEndReason.Missed,
        DomainCallEndReason.Busy => ContractCallEndReason.Busy,
        DomainCallEndReason.Failed => ContractCallEndReason.Failed,
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
    };

    private static void EnsurePayload(string payload, int maxLength, string name)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new HubException($"The {name} is required.");
        }

        if (payload.Length > maxLength)
        {
            throw new HubException($"The {name} is too large.");
        }
    }
}
