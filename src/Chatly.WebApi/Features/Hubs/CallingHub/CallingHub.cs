using System.Data.Common;
using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.WebApi.Features.Calls.Models;
using Chatly.WebApi.Features.Calls.Services;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Features.Hubs.CallingHub;

internal sealed class CallingHub(
    ChatlyDbContext context,
    CallService callService,
    LiveKitTokenFactory liveKitTokenFactory) : HubBase<ICallingHubClient>(context), ICallingHubServer
{
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
            var result = await Database.ExecuteSerializableAsync(async () =>
            {
                var receiver = await Database.Users.SingleOrDefaultAsync(
                                   user => user.Id == receiverUserId,
                                   cancellationToken)
                               ?? throw new HubException("The user was not found.");

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

                var call = new Call(callerUserId, receiverUserId);
                Database.Calls.Add(call);
                Database.ActiveCallParticipants.AddRange(
                    new ActiveCallParticipant(callerUserId, call.Id),
                    new ActiveCallParticipant(receiverUserId, call.Id));

                return (Call: call, Receiver: receiver);
            }, cancellationToken);

            var caller = await Database.Users.AsNoTracking()
                .SingleAsync(user => user.Id == callerUserId, cancellationToken);
            callService.ScheduleRingingExpiry(result.Call.Id);
            await callService.PublishAsync(receiverUserId, new IncomingCallNotification(
                result.Call.Id.Value,
                callerUserId.Value,
                caller.Username,
                CallRole.Receiver,
                CallState.Ringing));
            await callService.PublishToOtherDevicesAsync(callerUserId, Context.ConnectionId,
                new CallStateChangedNotification(
                    result.Call.Id.Value,
                    receiverUserId.Value,
                    result.Receiver.Username,
                    CallRole.Caller,
                    CallState.Ringing));

            return new CallInfo(
                result.Call.Id.Value,
                receiverUserId.Value,
                result.Receiver.Username,
                CallRole.Caller,
                CallState.Ringing,
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

        return call is null ? null : CallContractMapper.ToCallInfo(call, actorUserId);
    }

    public async Task<CallInfo> AcceptCallAsync(Guid callId)
    {
        var (call, actorUserId) = await TransitionAsync(callId, callService.AcceptAsync);
        var acceptedAt = call.AcceptedAt!.Value;
        await callService.PublishAsync(call.CallerUserId, new CallAcceptedNotification(
            call.Id.Value,
            call.ReceiverUserId.Value,
            call.ReceiverUser.Username,
            CallRole.Caller,
            CallState.Active,
            acceptedAt));
        await callService.PublishToOtherDevicesAsync(call.ReceiverUserId, Context.ConnectionId,
            new CallStateChangedNotification(
                call.Id.Value,
                call.CallerUserId.Value,
                call.CallerUser.Username,
                CallRole.Receiver,
                CallState.Active,
                acceptedAt));
        return CallContractMapper.ToCallInfo(call, actorUserId);
    }

    public async Task RejectCallAsync(Guid callId)
    {
        var (call, _) = await TransitionAsync(callId, callService.RejectAsync);
        await callService.PublishEndedAsync(call, static (callId, remoteId, remoteName, role, state, reason) =>
            new CallRejectedNotification(callId, remoteId, remoteName, role, state, reason));
    }

    public async Task EndCallAsync(Guid callId)
    {
        var (call, _) = await TransitionAsync(callId, callService.EndAsync);
        await callService.PublishEndedAsync(call, static (callId, remoteId, remoteName, role, state, reason) =>
            new CallEndedNotification(callId, remoteId, remoteName, role, state, reason));
    }

    public async Task<CallMediaAccess> JoinMediaAsync(Guid callId)
    {
        if (callId == Guid.Empty)
        {
            throw new HubException("The call ID is invalid.");
        }

        var cancellationToken = Context.ConnectionAborted;
        var actorUserId = await GetCurrentUserIdAsync(cancellationToken);
        var call = await Database.Calls
                       .AsNoTracking()
                       .Include(activeCall => activeCall.CallerUser)
                       .Include(activeCall => activeCall.ReceiverUser)
                       .SingleOrDefaultAsync(activeCall => activeCall.Id == CallId.From(callId), cancellationToken)
                   ?? throw new HubException("The call was not found.");

        try
        {
            CallService.EnsureCanJoinMedia(call, actorUserId);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or CallTransitionException)
        {
            throw new HubException(exception.Message);
        }

        var actor = actorUserId == call.CallerUserId ? call.CallerUser : call.ReceiverUser;
        return new CallMediaAccess(
            liveKitTokenFactory.ServerUrl,
            liveKitTokenFactory.CreateJoinToken(call.Id, actorUserId, actor.Username));
    }

    private async Task<(Call Call, UserId ActorUserId)> TransitionAsync(
        Guid callId,
        Func<CallId, UserId, CancellationToken, Task<Call?>> transition)
    {
        if (callId == Guid.Empty)
        {
            throw new HubException("The call ID is invalid.");
        }

        var cancellationToken = Context.ConnectionAborted;
        var actorUserId = await GetCurrentUserIdAsync(cancellationToken);

        try
        {
            var call = await transition(CallId.From(callId), actorUserId, cancellationToken);
            return (call ?? throw new HubException("The call was not found."), actorUserId);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or CallTransitionException)
        {
            throw new HubException(exception.Message);
        }
        catch (Exception exception) when (exception is DbUpdateException or DbException)
        {
            throw new HubException("The call could not be updated.");
        }
    }
}