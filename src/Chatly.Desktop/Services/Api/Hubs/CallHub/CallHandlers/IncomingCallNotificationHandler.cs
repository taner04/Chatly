using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Api.Hubs;
using Chatly.Desktop.Services.Calls;

namespace Chatly.Desktop.Services.Api.Hubs.CallHub.CallHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class IncomingCallNotificationHandler(CallSession session)
    : HubMessageHandler<IncomingCallNotification>
{
    protected override Task HandleMessageAsync(IncomingCallNotification message) =>
        session.EnqueueAsync(async () =>
        {
            if (session.HasDifferentCall(message.CallId))
            {
                return;
            }

            session.SetCall(message, false);
            await session.StopTonesAsync();
            if (message.Role == CallRole.Receiver)
            {
                await session.StartIncomingToneAsync();
            }
            else
            {
                await session.StartOutgoingToneAsync();
            }
        });
}
