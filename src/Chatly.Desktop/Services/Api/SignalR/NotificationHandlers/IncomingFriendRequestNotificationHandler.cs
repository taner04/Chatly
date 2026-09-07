using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.SignalR;
using Chatly.Desktop.Abstraction.Notification;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Utilities;
using Microsoft.Extensions.Logging;
using UserSessionContext = Chatly.Desktop.Models.UserSession.UserSessionContext;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
public sealed class IncomingFriendRequestNotificationHandler(
    UserSessionContext userSessionContext,
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
            if (!userSessionContext.AddFriendRequest(friendRequest))
            {
                return;
            }

            userSessionContext.IncrementPendingFriendRequestCount();
            toastService.AddNotification($"Friend request from '{message.SenderUsername}'");
            await notificationService.PlayNotificationSoundAsync();
        });
    }
}