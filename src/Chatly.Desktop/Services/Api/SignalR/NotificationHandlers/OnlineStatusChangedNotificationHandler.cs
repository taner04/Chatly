using Chatly.Contracts.SignalR;
using Chatly.Desktop.Utilities;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
public sealed class OnlineStatusChangedNotificationHandler(
    UserSessionContext sessionContext,
    ILogger<ClientNotificationHandler<OnlineStatusChangedMessage>> logger)
    : ClientNotificationHandler<OnlineStatusChangedMessage>(logger)
{
    public override NotificationType Type => NotificationType.OnlineStatusChanged;

    protected override Task HandleNotificationAsync(OnlineStatusChangedMessage message)
    {
        UiThreadDispatcher.SafeInvoke(() =>
            sessionContext.SetFriendOnlineStatus(message.UserId, message.IsOnline));
        return Task.CompletedTask;
    }
}