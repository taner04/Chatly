using Chatly.Contracts.Features.Messages.Endpoints.RemoveMessage;
using Chatly.Desktop.Utilities;
using ChatMessagesViewModel = Chatly.Desktop.ViewModels.Pages.ChatPage.Messages.ChatMessagesViewModel;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class MessageDeletedNotificationHandler(
    ChatMessagesViewModel messagesViewModel,
    ILogger<ClientNotificationHandler<MessageDeletedNotification>> logger)
    : ClientNotificationHandler<MessageDeletedNotification>(logger)
{
    protected override Task HandleNotificationAsync(MessageDeletedNotification message)
    {
        UIThreadDispatcher.SafeInvoke(() => messagesViewModel.ReceiveMessageDeleted(message));
        return Task.CompletedTask;
    }
}