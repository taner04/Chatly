using Chatly.Contracts.SignalR;
using Chatly.Desktop.Utilities;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationEventStrategies;

[SingletonService(typeof(INotificationStrategy))]
public sealed class NotificationFriendshipRemovedStrategy(
    UserSessionContext sessionContext,
    ILogger<NotificationStrategy<FriendshipRemovedMessage>> logger)
    : NotificationStrategy<FriendshipRemovedMessage>(logger)
{
    public override NotificationType Type => NotificationType.FriendshipRemoved;

    protected override Task HandleNotificationAsync(FriendshipRemovedMessage message)
    {
        DispatcherUtility.SafeInvoke(() => sessionContext.RemoveFriend(message.AssociatedUserId));
        return Task.CompletedTask;
    }
}
