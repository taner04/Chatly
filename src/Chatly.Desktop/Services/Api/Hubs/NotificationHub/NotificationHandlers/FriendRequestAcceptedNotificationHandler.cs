using Chatly.Contracts.Features.FriendRequests.Endpoints.AcceptFriendRequest;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Friendships;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class FriendRequestAcceptedNotificationHandler(
    FriendshipStateService friendshipStateService)
    : HubMessageHandler<FriendRequestAcceptedNotification>
{
    protected override Task HandleMessageAsync(FriendRequestAcceptedNotification message)
    {
        UiThreadDispatcher.SafeInvoke(() => { friendshipStateService.ApplyAccepted(message.Friendship); });
        return Task.CompletedTask;
    }
}