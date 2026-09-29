using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Calls;

namespace Chatly.Desktop.Services.Api.Hubs.CallHub.CallHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class CallRejectedNotificationHandler(CallSession session)
    : HubMessageHandler<CallRejectedNotification>
{
    protected override Task HandleMessageAsync(CallRejectedNotification message) =>
        session.EnqueueAsync(async () =>
        {
            if (session.TryApplyCurrentNotification(message, out _))
            {
                await session.TeardownAsync(message.CallId);
            }
        });
}