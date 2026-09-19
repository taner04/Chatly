using Chatly.Contracts.Features.FriendRequests.Endpoints.AcceptFriendRequest;
using Chatly.Desktop.Services.Friendships;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class FriendRequestAcceptedNotificationHandler(
    FriendshipStateService friendshipStateService,
    ILogger<ClientNotificationHandler<FriendRequestAcceptedNotification>> logger)
    : ClientNotificationHandler<FriendRequestAcceptedNotification>(logger)
{
    protected override Task HandleNotificationAsync(FriendRequestAcceptedNotification message)
    {
        UIThreadDispatcher.SafeInvoke(() => { friendshipStateService.ApplyAccepted(message.Friendship); });
        return Task.CompletedTask;
    }
}