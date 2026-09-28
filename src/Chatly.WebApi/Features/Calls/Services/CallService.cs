using Chatly.Contracts.Features.Hubs;
using Chatly.WebApi.Features.Calls.Jobs;
using Chatly.WebApi.Features.Calls.Models;
using Chatly.WebApi.Features.Hubs;
using Chatly.WebApi.Features.Hubs.CallingHub;
using Hangfire;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Features.Calls.Services;

[ScopedService]
internal sealed partial class CallService(
    ChatlyDbContext context,
    IHubContext<CallingHub, ICallingHubClient> hubContext,
    IBackgroundJobClient backgroundJobClient,
    ILogger<CallService> logger)
{
    internal Task<Call?> AcceptAsync(CallId callId, UserId actorUserId, CancellationToken cancellationToken) =>
        TryTransitionAsync(callId, call =>
        {
            EnsureParticipant(call, actorUserId);
            EnsureRole(actorUserId, call.ReceiverUserId, "Only the receiver can accept the call.");
            EnsureStatus(call, CallState.Ringing);
            call.Status = CallState.Active;
            call.AcceptedAt = DateTimeOffset.UtcNow;
            return true;
        }, false, cancellationToken);

    internal Task<Call?> RejectAsync(CallId callId, UserId actorUserId, CancellationToken cancellationToken) =>
        TryTransitionAsync(callId, call =>
        {
            EnsureParticipant(call, actorUserId);
            EnsureRole(actorUserId, call.ReceiverUserId, "Only the receiver can reject the call.");
            EnsureStatus(call, CallState.Ringing);
            Finish(call, CallEndReason.Declined, DateTimeOffset.UtcNow);
            return true;
        }, true, cancellationToken);

    internal Task<Call?> EndAsync(CallId callId, UserId actorUserId, CancellationToken cancellationToken) =>
        TryTransitionAsync(callId, call =>
        {
            EnsureParticipant(call, actorUserId);
            var reason = call.Status switch
            {
                CallState.Ringing when actorUserId == call.CallerUserId => CallEndReason.Cancelled,
                CallState.Ringing => CallEndReason.Declined,
                CallState.Active => CallEndReason.Completed,
                _ => throw new CallTransitionException("The call has already ended.")
            };
            Finish(call, reason, DateTimeOffset.UtcNow);
            return true;
        }, true, cancellationToken);

    internal Task<Call?> ExpireAsync(CallId callId, CancellationToken cancellationToken) =>
        TryTransitionAsync(callId, call =>
        {
            var now = DateTimeOffset.UtcNow;
            CallEndReason? reason = call switch
            {
                { Status: CallState.Ringing } when call.InitiatedAt <= now - CallExpiryJob.RingingTimeout =>
                    CallEndReason.Missed,
                { Status: CallState.Active } when call.InitiatedAt <= now - CallExpiryJob.MaximumCallLifetime =>
                    CallEndReason.Failed,
                _ => null
            };

            if (reason is not { } endReason)
            {
                return false;
            }

            Finish(call, endReason, now);
            return true;
        }, true, cancellationToken);

    internal Task<Call?> AbandonAsync(CallId callId, CancellationToken cancellationToken) =>
        TryTransitionAsync(callId, call =>
        {
            if (call.Status is not CallState.Active)
            {
                return false;
            }

            Finish(call, CallEndReason.Failed, DateTimeOffset.UtcNow);
            return true;
        }, true, cancellationToken);

    internal static void EnsureCanJoinMedia(Call call, UserId actorUserId)
    {
        EnsureParticipant(call, actorUserId);
        EnsureStatus(call, CallState.Active);
    }

    private Task<Call?> TryTransitionAsync(
        CallId callId,
        Func<Call, bool> transition,
        bool releaseReservations,
        CancellationToken cancellationToken) =>
        context.ExecuteSerializableAsync(async () =>
        {
            var call = await context.Calls
                .Include(candidate => candidate.CallerUser)
                .Include(candidate => candidate.ReceiverUser)
                .Include(candidate => candidate.ActiveParticipants)
                .SingleOrDefaultAsync(candidate => candidate.Id == callId, cancellationToken);

            if (call is null || !transition(call))
            {
                return null;
            }

            if (releaseReservations)
            {
                context.ActiveCallParticipants.RemoveRange(call.ActiveParticipants);
            }

            return call;
        }, cancellationToken);

    internal Task PublishEndedAsync(
        Call call,
        Func<Guid, Guid, string?, CallRole, CallState, CallEndReason, CallMessage> notification)
    {
        var reason = call.EndReason!.Value;
        return Task.WhenAll(
            PublishAsync(call.CallerUserId, notification(
                call.Id.Value,
                call.ReceiverUserId.Value,
                call.ReceiverUser.Username,
                CallRole.Caller,
                CallState.Ended,
                reason)),
            PublishAsync(call.ReceiverUserId, notification(
                call.Id.Value,
                call.CallerUserId.Value,
                call.CallerUser.Username,
                CallRole.Receiver,
                CallState.Ended,
                reason)));
    }

    internal async Task PublishAsync(UserId userId, CallMessage notification)
    {
        try
        {
            await hubContext.Clients.Group(HubGroups.User(userId)).Receive(notification);
        }
        catch (Exception exception)
        {
            LogPublishFailed(exception, notification.CallId, userId);
        }
    }

    internal async Task PublishToOtherDevicesAsync(UserId userId, string connectionId, CallMessage notification)
    {
        try
        {
            await hubContext.Clients.GroupExcept(HubGroups.User(userId), [connectionId]).Receive(notification);
        }
        catch (Exception exception)
        {
            LogPublishToOtherDevicesFailed(exception, notification.CallId, userId);
        }
    }

    internal void ScheduleRingingExpiry(CallId callId)
    {
        try
        {
            backgroundJobClient.Schedule<CallExpiryJob>(
                job => job.ExecuteAsync(callId.Value, CancellationToken.None),
                CallExpiryJob.RingingTimeout);
        }
        catch (Exception exception)
        {
            LogExpiryScheduleFailed(exception, callId.Value);
        }
    }

    private static void Finish(Call call, CallEndReason reason, DateTimeOffset timestamp)
    {
        call.Status = CallState.Ended;
        call.EndReason = reason;
        call.EndedAt = timestamp;
    }

    private static void EnsureParticipant(Call call, UserId actorUserId)
    {
        if (actorUserId != call.CallerUserId && actorUserId != call.ReceiverUserId)
        {
            throw new UnauthorizedAccessException("The user is not a participant in this call.");
        }
    }

    private static void EnsureRole(UserId actorUserId, UserId expectedUserId, string message)
    {
        if (actorUserId != expectedUserId)
        {
            throw new UnauthorizedAccessException(message);
        }
    }

    private static void EnsureStatus(Call call, CallState expectedStatus)
    {
        if (call.Status != expectedStatus)
        {
            throw new CallTransitionException($"The call must be {expectedStatus}.");
        }
    }

    [LoggerMessage(LogLevel.Warning, "Failed to publish call {CallId} to user {UserId}.")]
    private partial void LogPublishFailed(Exception exception, Guid callId, UserId userId);

    [LoggerMessage(LogLevel.Warning, "Failed to publish call {CallId} to other devices of user {UserId}.")]
    private partial void LogPublishToOtherDevicesFailed(Exception exception, Guid callId, UserId userId);

    [LoggerMessage(LogLevel.Error, "Failed to schedule ringing expiry for call {CallId}.")]
    private partial void LogExpiryScheduleFailed(Exception exception, Guid callId);
}
