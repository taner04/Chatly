using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.SignalR;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Utilities;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
public sealed class FriendRequestAcceptedNotificationHandler(
    UserSessionContext sessionContext,
    ILogger<ClientNotificationHandler<FriendRequestAcceptedMessage>> logger)
    : ClientNotificationHandler<FriendRequestAcceptedMessage>(logger)
{
    public override NotificationType Type => NotificationType.FriendRequestAccepted;

    protected override Task HandleNotificationAsync(FriendRequestAcceptedMessage message)
    {
        UiThreadDispatcher.SafeInvoke(() =>
        {
            sessionContext.AddFriend(FriendMapper.Map(message));
            sessionContext.AddDirectChat(DirectChatMapper.Map(message));
        });
        return Task.CompletedTask;
    }
}