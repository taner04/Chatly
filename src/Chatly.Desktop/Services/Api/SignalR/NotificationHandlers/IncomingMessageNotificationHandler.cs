using Chatly.Contracts.Features.Messages.Endpoints.SendMessage;
using Chatly.Desktop.Abstraction.Notification;
using Chatly.Desktop.Utilities;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using ChatMessagesViewModel = Chatly.Desktop.ViewModels.Pages.ChatPage.Messages.ChatMessagesViewModel;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class IncomingMessageNotificationHandler(
    ChatSidebarViewModel chatSidebar,
    ChatPageViewModel chatPage,
    ChatMessagesViewModel messagesViewModel,
    ChatTypingViewModel typingViewModel,
    INotificationSoundPlayer notificationSoundPlayer,
    ILogger<ClientNotificationHandler<IncomingMessageNotification>> logger)
    : ClientNotificationHandler<IncomingMessageNotification>(logger)
{
    protected override Task HandleNotificationAsync(IncomingMessageNotification message)
    {
        return UIThreadDispatcher.SafeInvokeAsync(async () =>
        {
            if (messagesViewModel.ReceiveIncomingMessage(message))
            {
                typingViewModel.ResetOtherUserTyping();
                await chatPage.MarkChatReadAsync(message.Message.ChatId);
            }
            else
            {
                chatSidebar.ReceiveIncomingMessage(message.Message.ChatId);
            }

            await notificationSoundPlayer.PlayNotificationSoundAsync();
        });
    }
}