using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.WebApi.Features.Calls.Enums;
using Chatly.WebApi.Features.Calls.Models;
using Chatly.WebApi.Features.Calls.Services;
using DomainCall = Chatly.WebApi.Features.Calls.Models.Call;

namespace Chatly.WebApi.Features.Calls.Jobs;

[ScopedService]
internal sealed class CallExpiryJob(CallService callService)
{
    internal static readonly TimeSpan RingingTimeout = TimeSpan.FromSeconds(45);
    internal static readonly TimeSpan MaximumCallLifetime = TimeSpan.FromHours(12);

    public Task ExecuteAsync(Guid callId, CancellationToken cancellationToken = default) =>
        EndAsync(callId, TryExpire, cancellationToken);

    internal Task AbandonAsync(Guid callId, CancellationToken cancellationToken = default) =>
        EndAsync(callId, TryAbandon, cancellationToken);

    private static bool TryExpire(DomainCall call, DateTimeOffset now)
    {
        if (call.Status == CallStatus.Ringing && call.InitiatedAt <= now - RingingTimeout)
        {
            call.Expire(now);
            return true;
        }

        if (call.Status == CallStatus.Active && call.InitiatedAt <= now - MaximumCallLifetime)
        {
            call.ExpireAbandoned(now);
            return true;
        }

        return false;
    }

    private static bool TryAbandon(DomainCall call, DateTimeOffset now)
    {
        if (call.Status != CallStatus.Active)
        {
            return false;
        }

        call.ExpireAbandoned(now);
        return true;
    }

    private async Task EndAsync(
        Guid callId,
        Func<DomainCall, DateTimeOffset, bool> tryEnd,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var ended = await callService.TryTransitionAsync(
            CallId.From(callId),
            call => call.Status != CallStatus.Ended && tryEnd(call, now),
            true,
            cancellationToken);

        if (ended is not null)
        {
            await callService.PublishEndedAsync(ended, static (callId, remoteId, remoteName, role, state, reason) =>
                new CallEndedNotification(callId, remoteId, remoteName, role, state, reason));
        }
    }
}
