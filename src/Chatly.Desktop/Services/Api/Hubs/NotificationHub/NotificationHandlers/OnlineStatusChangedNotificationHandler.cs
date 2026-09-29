using Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class OnlineStatusChangedNotificationHandler(
    UserRegistry userRegistry)
    : HubMessageHandler<OnlineStatusChangedNotification>
{
    protected override Task HandleMessageAsync(OnlineStatusChangedNotification message)
    {
        UiThreadDispatcher.SafeInvoke(() => { userRegistry.SetOnlineStatus(message.UserId, message.IsOnline); });
        return Task.CompletedTask;
    }
}