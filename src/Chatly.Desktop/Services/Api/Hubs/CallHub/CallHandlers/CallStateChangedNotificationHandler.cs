using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Calls;

namespace Chatly.Desktop.Services.Api.Hubs.CallHub.CallHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class CallStateChangedNotificationHandler(CallSession session)
    : HubMessageHandler<CallStateChangedNotification>
{
    protected override Task HandleMessageAsync(CallStateChangedNotification message) =>
        session.EnqueueAsync(async () =>
        {
            if (session.HasDifferentCall(message.CallId))
            {
                return;
            }

            session.SetCall(message, true);
            await session.StopTonesAsync();
        });
}