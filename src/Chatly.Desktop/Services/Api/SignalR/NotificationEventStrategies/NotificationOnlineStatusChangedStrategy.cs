using Chatly.Contracts.SignalR;
using Chatly.Desktop.Utilities;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationEventStrategies;

[SingletonService(typeof(INotificationStrategy))]
public sealed class NotificationOnlineStatusChangedStrategy(
    UserSessionContext sessionContext,
    ILogger<NotificationStrategy<OnlineStatusChangedMessage>> logger)
    : NotificationStrategy<OnlineStatusChangedMessage>(logger)
{
    public override NotificationType Type => NotificationType.OnlineStatusChanged;

    protected override Task HandleNotificationAsync(OnlineStatusChangedMessage message)
    {
        DispatcherUtility.SafeInvoke(() =>
            sessionContext.SetFriendOnlineStatus(message.UserId, message.IsOnline));
        return Task.CompletedTask;
    }
}
