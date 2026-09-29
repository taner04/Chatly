using Chatly.Contracts.Features.Reactions.Notifications;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Utilities;
using ChatMessagesViewModel = Chatly.Desktop.ViewModels.Pages.ChatPage.Messages.ChatMessagesViewModel;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class ReactionChangedNotificationHandler(
    ChatMessagesViewModel messagesViewModel)
    : HubMessageHandler<ReactionChangedNotification>
{
    protected override Task HandleMessageAsync(ReactionChangedNotification message)
    {
        UiThreadDispatcher.SafeInvoke(() => messagesViewModel.ReceiveReactionChanged(message));
        return Task.CompletedTask;
    }
}