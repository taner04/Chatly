using Chatly.Contracts.SignalR;
using Chatly.Desktop.Models.UserSession;
using Chatly.Desktop.Utilities;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class OnlineStatusChangedNotificationHandler(
    FriendState friendState,
    DirectChatState directChatState,
    ILogger<ClientNotificationHandler<OnlineStatusChangedMessage>> logger)
    : ClientNotificationHandler<OnlineStatusChangedMessage>(logger)
{
    public override NotificationType Type => NotificationType.OnlineStatusChanged;

    protected override Task HandleNotificationAsync(OnlineStatusChangedMessage message)
    {
        UiThreadDispatcher.SafeInvoke(() =>
        {
            friendState.SetOnlineStatus(message.UserId, message.IsOnline);
            directChatState.SetOnlineStatus(message.UserId, message.IsOnline);
        });
        return Task.CompletedTask;
    }
}
