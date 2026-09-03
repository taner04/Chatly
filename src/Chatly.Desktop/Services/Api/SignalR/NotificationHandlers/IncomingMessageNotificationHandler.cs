using Chatly.Contracts.Endpoints.Messages.Results;
using Chatly.Contracts.SignalR;
using Chatly.Desktop.Utilities;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
public sealed class IncomingMessageNotificationHandler(
    ChatSidebarViewModel chatSidebar,
    ChatPageViewModel chatPage,
    ILogger<ClientNotificationHandler<IncomingChatMessage>> logger)
    : ClientNotificationHandler<IncomingChatMessage>(logger)
{
    public override NotificationType Type => NotificationType.IncomingMessage;

    protected override Task HandleNotificationAsync(IncomingChatMessage message)
    {
        UiThreadDispatcher.SafeInvoke(() =>
        {
            if (!chatPage.ReceiveIncomingMessage(message))
            {
                chatSidebar.ReceiveIncomingMessage(message.ChatId);
            }
        });

        return Task.CompletedTask;
    }
}