using Chatly.Contracts.Features.Hubs.Notifications;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class OnlineStatusChangedNotificationHandler(
    UserRegistry userRegistry,
    ILogger<ClientNotificationHandler<OnlineStatusChangedNotification>> logger)
    : ClientNotificationHandler<OnlineStatusChangedNotification>(logger)
{
    protected override Task HandleNotificationAsync(OnlineStatusChangedNotification message)
    {
        UIThreadDispatcher.SafeInvoke(() => { userRegistry.SetOnlineStatus(message.UserId, message.IsOnline); });
        return Task.CompletedTask;
    }
}