using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.SignalR;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Utilities;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationEventStrategies;

[SingletonService(typeof(INotificationStrategy))]
public sealed class NotificationFriendRequestAcceptedStrategy(
    UserSessionContext sessionContext,
    ILogger<NotificationStrategy<FriendRequestAcceptedMessage>> logger)
    : NotificationStrategy<FriendRequestAcceptedMessage>(logger)
{
    public override NotificationType Type => NotificationType.FriendRequestAccepted;

    protected override Task HandleNotificationAsync(FriendRequestAcceptedMessage message)
    {
        DispatcherUtility.SafeInvoke(() =>
        {
            sessionContext.AddFriend(FriendMapper.Map(message));
            sessionContext.AddDirectChat(DirectChatMapper.Map(message));
        });
        return Task.CompletedTask;
    }
}
