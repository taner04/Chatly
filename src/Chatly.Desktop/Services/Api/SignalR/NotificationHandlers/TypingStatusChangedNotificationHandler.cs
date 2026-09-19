using Chatly.Contracts.Features.Hubs.Notifications;
using Chatly.Desktop.Utilities;
using Chatly.Desktop.ViewModels.Pages.ChatPage;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class TypingStatusChangedNotificationHandler(
    ChatTypingViewModel typingViewModel,
    ILogger<ClientNotificationHandler<TypingStatusChangedNotification>> logger)
    : ClientNotificationHandler<TypingStatusChangedNotification>(logger)
{
    protected override Task HandleNotificationAsync(TypingStatusChangedNotification message)
    {
        UIThreadDispatcher.SafeInvoke(() => typingViewModel.ReceiveTypingStatus(message));
        return Task.CompletedTask;
    }
}