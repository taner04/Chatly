using Chatly.Contracts.Features.Messages.Endpoints.RemoveMessage;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Utilities;
using ChatMessagesViewModel = Chatly.Desktop.ViewModels.Pages.ChatPage.Messages.ChatMessagesViewModel;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler<Notification>))]
internal sealed class MessageDeletedNotificationHandler(
    ChatMessagesViewModel messagesViewModel,
    ILogger<NotificationHandler<MessageDeletedNotification>> logger)
    : NotificationHandler<MessageDeletedNotification>(logger)
{
    protected override Task HandleNotificationAsync(MessageDeletedNotification message)
    {
        UiThreadDispatcher.SafeInvoke(() => messagesViewModel.ReceiveMessageDeleted(message));
        return Task.CompletedTask;
    }
}