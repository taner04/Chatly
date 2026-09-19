using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Desktop.Abstraction.Notification;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class IncomingFriendRequestNotificationHandler(
    FriendRequestState friendRequestState,
    IToastService toastService,
    INotificationSoundPlayer notificationSoundPlayer,
    ILogger<ClientNotificationHandler<IncomingFriendRequestNotification>> logger)
    : ClientNotificationHandler<IncomingFriendRequestNotification>(logger)
{
    protected override Task HandleNotificationAsync(IncomingFriendRequestNotification message)
    {
        return UIThreadDispatcher.SafeInvokeAsync(async () =>
        {
            var friendRequest = FriendRequestMapper.Map(message);
            if (!friendRequestState.Add(friendRequest))
            {
                return;
            }

            friendRequestState.IncrementPendingCount();
            toastService.AddNotification($"Friend request from '{message.Request.SenderUsername}'");
            await notificationSoundPlayer.PlayNotificationSoundAsync();
        });
    }
}