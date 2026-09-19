using Chatly.Contracts.Features.Reactions.Notifications;
using Chatly.Desktop.Utilities;
using ChatMessagesViewModel = Chatly.Desktop.ViewModels.Pages.ChatPage.Messages.ChatMessagesViewModel;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class ReactionChangedNotificationHandler(
    ChatMessagesViewModel messagesViewModel,
    ILogger<ClientNotificationHandler<ReactionChangedNotification>> logger)
    : ClientNotificationHandler<ReactionChangedNotification>(logger)
{
    protected override Task HandleNotificationAsync(ReactionChangedNotification message)
    {
        UIThreadDispatcher.SafeInvoke(() => messagesViewModel.ReceiveReactionChanged(message));
        return Task.CompletedTask;
    }
}