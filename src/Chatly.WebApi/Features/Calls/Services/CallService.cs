using System.Data;
using Chatly.Contracts.Features.Hubs;
using Chatly.WebApi.Features.Calls.Jobs;
using Chatly.WebApi.Features.Calls.Models;
using Chatly.WebApi.Features.Hubs;
using Chatly.WebApi.Features.Hubs.CallingHub;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using ContractCall = Chatly.Contracts.Features.Hubs.Call;
using ContractCallEndReason = Chatly.Contracts.Features.Hubs.CallEndReason;
using DomainCall = Chatly.WebApi.Features.Calls.Models.Call;

namespace Chatly.WebApi.Features.Calls.Services;

[ScopedService]
internal sealed partial class CallService(
    ChatlyDbContext context,
    IHubContext<CallingHub, ICallingHubClient> hubContext,
    IBackgroundJobClient backgroundJobClient,
    ILogger<CallService> logger)
{
    internal async Task<T> ExecuteSerializableAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            context.ChangeTracker.Clear();
            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            var result = await operation();
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }

    internal Task<DomainCall?> TryTransitionAsync(
        CallId callId,
        Func<DomainCall, bool> transition,
        bool releaseReservations,
        CancellationToken cancellationToken) =>
        ExecuteSerializableAsync(async () =>
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
        DomainCall call,
        Func<Guid, Guid, string?, CallRole, CallState, ContractCallEndReason, ContractCall> notification)
    {
        var reason = CallContractMapper.ToContract(call.EndReason!.Value);
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

    internal async Task PublishAsync(UserId userId, ContractCall notification)
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

    internal async Task PublishToOtherDevicesAsync(UserId userId, string connectionId, ContractCall notification)
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

    [LoggerMessage(LogLevel.Warning, "Failed to publish call {CallId} to user {UserId}.")]
    private partial void LogPublishFailed(Exception exception, Guid callId, UserId userId);

    [LoggerMessage(LogLevel.Warning, "Failed to publish call {CallId} to other devices of user {UserId}.")]
    private partial void LogPublishToOtherDevicesFailed(Exception exception, Guid callId, UserId userId);

    [LoggerMessage(LogLevel.Error, "Failed to schedule ringing expiry for call {CallId}.")]
    private partial void LogExpiryScheduleFailed(Exception exception, Guid callId);
}
