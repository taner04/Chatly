using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Api.Hubs;
using Chatly.Desktop.Services.Calls;

namespace Chatly.Desktop.Services.Api.Hubs.CallHub.CallHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class CallAcceptedNotificationHandler(CallSession session)
    : HubMessageHandler<CallAcceptedNotification>
{
    protected override Task HandleMessageAsync(CallAcceptedNotification message) =>
        session.EnqueueAsync(async () =>
        {
            if (!session.TryApplyCurrentNotification(message, out var isOnAnotherDevice)
                || isOnAnotherDevice)
            {
                return;
            }

            await session.StopTonesAsync();
            await session.EnsureMediaAsync(CancellationToken.None);
        });
}
