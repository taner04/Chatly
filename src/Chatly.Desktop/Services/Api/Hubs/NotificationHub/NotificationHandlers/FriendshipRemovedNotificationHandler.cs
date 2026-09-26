using Chatly.Contracts.Features.Friendships.Endpoints.RemoveFriendship;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Api.Hubs;
using Chatly.Desktop.Services.Friendships;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class FriendshipRemovedNotificationHandler(
    FriendshipStateService friendshipStateService,
    IToastService toastService)
    : HubMessageHandler<FriendshipRemovedNotification>
{
    protected override Task HandleMessageAsync(FriendshipRemovedNotification message)
    {
        return UiThreadDispatcher.SafeInvokeAsync(async () =>
        {
            var username = await friendshipStateService.ApplyRemovedAsync(message.AssociatedUserId) ?? "Unknown User";
            toastService.AddNotification($"{username} removed you as a friend.");
        });
    }
}
