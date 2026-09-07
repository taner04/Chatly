using Chatly.Contracts.Endpoints.Messages.Results;
using Chatly.Contracts.SignalR;
using Chatly.Desktop.Abstraction.Notification;
using Chatly.Desktop.Utilities;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
public sealed class IncomingMessageNotificationHandler(
    ChatSidebarViewModel chatSidebar,
    ChatPageViewModel chatPage,
    INotificationService notificationService,
    ILogger<ClientNotificationHandler<IncomingChatMessage>> logger)
    : ClientNotificationHandler<IncomingChatMessage>(logger)
{
    public override NotificationType Type => NotificationType.IncomingMessage;

    protected override Task HandleNotificationAsync(IncomingChatMessage message)
    {
        return UiThreadDispatcher.SafeInvokeAsync(async () =>
        {
            if (chatPage.ReceiveIncomingMessage(message))
            {
                await chatPage.MarkChatReadAsync(message.ChatId);
            }
            else
            {
                chatSidebar.ReceiveIncomingMessage(message.ChatId);
            }

            await notificationService.PlayNotificationSoundAsync();
        });
    }
}