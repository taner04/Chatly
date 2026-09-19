using Chatly.Contracts.Features.Friendships.Endpoints.RemoveFriendship;
using Chatly.Desktop.Services.Friendships;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class FriendshipRemovedNotificationHandler(
    FriendshipStateService friendshipStateService,
    IToastService toastService,
    ILogger<ClientNotificationHandler<FriendshipRemovedNotification>> logger)
    : ClientNotificationHandler<FriendshipRemovedNotification>(logger)
{
    protected override Task HandleNotificationAsync(FriendshipRemovedNotification message)
    {
        return UIThreadDispatcher.SafeInvokeAsync(async () =>
        {
            var username = await friendshipStateService.ApplyRemovedAsync(message.AssociatedUserId)
                           ?? "Unknown User";
            toastService.AddNotification($"{username} removed you as a friend.");
        });
    }
}