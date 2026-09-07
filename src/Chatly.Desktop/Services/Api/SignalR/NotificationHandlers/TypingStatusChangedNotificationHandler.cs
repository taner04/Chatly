using Chatly.Contracts.SignalR;
using Chatly.Desktop.Utilities;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
public sealed class TypingStatusChangedNotificationHandler(
    ChatPageViewModel chatPage,
    ILogger<ClientNotificationHandler<TypingStatusChangedMessage>> logger)
    : ClientNotificationHandler<TypingStatusChangedMessage>(logger)
{
    public override NotificationType Type => NotificationType.TypingStatusChanged;

    protected override Task HandleNotificationAsync(TypingStatusChangedMessage message)
    {
        UiThreadDispatcher.SafeInvoke(() => chatPage.ReceiveTypingStatus(message));
        return Task.CompletedTask;
    }
}