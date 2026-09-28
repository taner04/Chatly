using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.WebApi.Features.Calls.Models;
using Chatly.WebApi.Features.Calls.Services;

namespace Chatly.WebApi.Features.Calls.Jobs;

[ScopedService]
internal sealed class CallExpiryJob(CallService callService)
{
    internal static readonly TimeSpan RingingTimeout = TimeSpan.FromSeconds(45);
    internal static readonly TimeSpan MaximumCallLifetime = TimeSpan.FromHours(12);

    public async Task ExecuteAsync(Guid callId, CancellationToken cancellationToken = default) =>
        await PublishEndedAsync(await callService.ExpireAsync(CallId.From(callId), cancellationToken));

    internal async Task AbandonAsync(Guid callId, CancellationToken cancellationToken = default) =>
        await PublishEndedAsync(await callService.AbandonAsync(CallId.From(callId), cancellationToken));

    private async Task PublishEndedAsync(Call? ended)
    {
        if (ended is not null)
        {
            await callService.PublishEndedAsync(ended, static (callId, remoteId, remoteName, role, state, reason) =>
                new CallEndedNotification(callId, remoteId, remoteName, role, state, reason));
        }
    }
}
