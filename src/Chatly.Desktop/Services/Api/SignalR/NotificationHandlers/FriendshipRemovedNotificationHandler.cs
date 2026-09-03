using System.Linq;
using Chatly.Contracts.SignalR;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Utilities;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
public sealed class FriendshipRemovedNotificationHandler(
    UserSessionContext sessionContext,
    ChatPageViewModel chatPageViewModel,
    IToastService toastService,
    ILogger<ClientNotificationHandler<FriendshipRemovedMessage>> logger)
    : ClientNotificationHandler<FriendshipRemovedMessage>(logger)
{
    public override NotificationType Type => NotificationType.FriendshipRemoved;

    protected override Task HandleNotificationAsync(FriendshipRemovedMessage message)
    {
        return UiThreadDispatcher.SafeInvokeAsync(async () =>
        {
            var friend =
                sessionContext.Friends.FirstOrDefault(existing => existing.User.Id == message.AssociatedUserId);
            var directChat =
                sessionContext.DirectChats.FirstOrDefault(chat => chat.User.Id == message.AssociatedUserId);
            var username = friend?.User.Username ?? directChat?.User.Username ?? "Unknown User";

            if (directChat is not null)
            {
                await chatPageViewModel.CloseChatAsync(directChat.Id);
            }

            sessionContext.RemoveFriend(message.AssociatedUserId);
            toastService.AddNotification($"{username} removed you as a friend.");
        });
    }
}