using System.Data;
using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.WebApi.Features.Calls.Enums;
using Chatly.WebApi.Features.Calls.Models;
using Chatly.WebApi.Features.Hubs;
using Chatly.WebApi.Features.Hubs.CallingHub;
using Microsoft.AspNetCore.SignalR;
using ContractCallEndReason = Chatly.Contracts.Features.Hubs.CallEndReason;
using DomainCallEndReason = Chatly.WebApi.Features.Calls.Enums.CallEndReason;

namespace Chatly.WebApi.Features.Calls.Jobs;

[ScopedService]
internal sealed class CallExpiryJob(
    ChatlyDbContext context,
    IHubContext<CallingHub, ICallingHubClient> hubContext,
    ILogger<CallExpiryJob> logger)
{
    internal static readonly TimeSpan RingingTimeout = TimeSpan.FromSeconds(45);
    internal static readonly TimeSpan MaximumCallLifetime = TimeSpan.FromHours(12);

    public async Task ExecuteAsync(Guid callId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var strategy = context.Database.CreateExecutionStrategy();
        var expired = await strategy.ExecuteAsync(async () =>
        {
            context.ChangeTracker.Clear();
            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            var call = await context.Calls
                .Include(candidate => candidate.CallerUser)
                .Include(candidate => candidate.ReceiverUser)
                .Include(candidate => candidate.ActiveParticipants)
                .SingleOrDefaultAsync(candidate => candidate.Id == CallId.From(callId), cancellationToken);

            if (call is null || call.Status == CallStatus.Ended)
            {
                return null;
            }

            if (call.Status == CallStatus.Ringing && call.InitiatedAt <= now - RingingTimeout)
            {
                call.Expire(now);
            }
            else if (call.Status != CallStatus.Ringing && call.InitiatedAt <= now - MaximumCallLifetime)
            {
                call.ExpireAbandoned(now);
            }
            else
            {
                return null;
            }

            context.ActiveCallParticipants.RemoveRange(call.ActiveParticipants);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return call;
        });

        if (expired is null)
        {
            return;
        }

        var reason = expired.EndReason == DomainCallEndReason.Missed
            ? ContractCallEndReason.Missed
            : ContractCallEndReason.Failed;
        await Task.WhenAll(
            PublishAsync(expired.CallerUserId, new CallEndedNotification(
                expired.Id.Value,
                expired.ReceiverUserId.Value,
                expired.ReceiverUser.Username,
                CallRole.Caller,
                CallState.Ended,
                reason)),
            PublishAsync(expired.ReceiverUserId, new CallEndedNotification(
                expired.Id.Value,
                expired.CallerUserId.Value,
                expired.CallerUser.Username,
                CallRole.Receiver,
                CallState.Ended,
                reason)));
    }

    private async Task PublishAsync(UserId userId, CallEndedNotification notification)
    {
        try
        {
            await hubContext.Clients.Group(HubGroups.User(userId)).Receive(notification);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to publish expiry for call {CallId} to user {UserId}.", notification.CallId, userId);
        }
    }
}
