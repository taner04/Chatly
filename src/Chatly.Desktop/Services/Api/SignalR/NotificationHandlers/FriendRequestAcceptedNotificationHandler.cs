using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.SignalR;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Models.UserSession;
using Chatly.Desktop.Utilities;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class FriendRequestAcceptedNotificationHandler(
    FriendState friendState,
    DirectChatState directChatState,
    ILogger<ClientNotificationHandler<FriendRequestAcceptedMessage>> logger)
    : ClientNotificationHandler<FriendRequestAcceptedMessage>(logger)
{
    public override NotificationType Type => NotificationType.FriendRequestAccepted;

    protected override Task HandleNotificationAsync(FriendRequestAcceptedMessage message)
    {
        UiThreadDispatcher.SafeInvoke(() =>
        {
            friendState.Add(FriendMapper.Map(message));
            directChatState.Add(DirectChatMapper.Map(message));
        });
        return Task.CompletedTask;
    }
}
