using Chatly.Contracts.Features.FriendRequests.Endpoints.AcceptFriendRequest;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Friendships;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler<Notification>))]
internal sealed class FriendRequestAcceptedNotificationHandler(
    FriendshipStateService friendshipStateService,
    ILogger<NotificationHandler<FriendRequestAcceptedNotification>> logger)
    : NotificationHandler<FriendRequestAcceptedNotification>(logger)
{
    protected override Task HandleNotificationAsync(FriendRequestAcceptedNotification message)
    {
        UiThreadDispatcher.SafeInvoke(() => { friendshipStateService.ApplyAccepted(message.Friendship); });
        return Task.CompletedTask;
    }
}