using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Abstraction.Notification;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Services.Api.Hubs;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class IncomingFriendRequestNotificationHandler(
    FriendRequestState friendRequestState,
    IToastService toastService,
    INotificationSoundPlayer notificationSoundPlayer)
    : HubMessageHandler<IncomingFriendRequestNotification>
{
    protected override Task HandleMessageAsync(IncomingFriendRequestNotification message)
    {
        return UiThreadDispatcher.SafeInvokeAsync(async () =>
        {
            var friendRequest = FriendRequestMapper.Map(message);
            if (!friendRequestState.Add(friendRequest))
            {
                return;
            }

            friendRequestState.IncrementPendingCount();
            toastService.AddNotification($"Friend request from '{message.Request.SenderUsername}'");
            await notificationSoundPlayer.PlayNotificationAsync();
        });
    }
}
