using Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler<Notification>))]
internal sealed class OnlineStatusChangedNotificationHandler(
    UserRegistry userRegistry,
    ILogger<NotificationHandler<OnlineStatusChangedNotification>> logger)
    : NotificationHandler<OnlineStatusChangedNotification>(logger)
{
    protected override Task HandleNotificationAsync(OnlineStatusChangedNotification message)
    {
        UiThreadDispatcher.SafeInvoke(() => { userRegistry.SetOnlineStatus(message.UserId, message.IsOnline); });
        return Task.CompletedTask;
    }
}