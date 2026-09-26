using Chatly.Contracts.Features.Messages.Endpoints.SendMessage;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Abstraction.Notification;
using Chatly.Desktop.Services.Api.Hubs;
using Chatly.Desktop.Utilities;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using ChatMessagesViewModel = Chatly.Desktop.ViewModels.Pages.ChatPage.Messages.ChatMessagesViewModel;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class IncomingMessageNotificationHandler(
    ChatSidebarViewModel chatSidebar,
    ChatPageViewModel chatPage,
    ChatMessagesViewModel messagesViewModel,
    ChatTypingViewModel typingViewModel,
    INotificationSoundPlayer notificationSoundPlayer)
    : HubMessageHandler<IncomingMessageNotification>
{
    protected override Task HandleMessageAsync(IncomingMessageNotification message)
    {
        return UiThreadDispatcher.SafeInvokeAsync(async () =>
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

            await notificationSoundPlayer.PlayNotificationAsync();
        });
    }
}
