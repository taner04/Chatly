using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.SignalR;
using Chatly.Desktop.Abstraction.Notification;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Models.UserSession;
using Chatly.Desktop.Utilities;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class IncomingFriendRequestNotificationHandler(
    FriendRequestState friendRequestState,
    IToastService toastService,
    INotificationService notificationService,
    ILogger<ClientNotificationHandler<IncomingFriendRequestMessage>> logger)
    : ClientNotificationHandler<IncomingFriendRequestMessage>(logger)
{
    public override NotificationType Type => NotificationType.IncomingFriendRequest;

    protected override Task HandleNotificationAsync(IncomingFriendRequestMessage message)
    {
        return UiThreadDispatcher.SafeInvokeAsync(async () =>
        {
            var friendRequest = FriendRequestMapper.Map(message);
            if (!friendRequestState.Add(friendRequest))
            {
                return;
            }

            friendRequestState.IncrementPendingCount();
            toastService.AddNotification($"Friend request from '{message.SenderUsername}'");
            await notificationService.PlayNotificationSoundAsync();
        });
    }
}
