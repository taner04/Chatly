using Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Utilities;
using Chatly.Desktop.ViewModels.Pages.ChatPage;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class TypingStatusChangedNotificationHandler(
    ChatTypingViewModel typingViewModel) : HubMessageHandler<TypingStatusChangedNotification>
{
    protected override Task HandleMessageAsync(TypingStatusChangedNotification message)
    {
        UiThreadDispatcher.SafeInvoke(() => typingViewModel.ReceiveTypingStatus(message));
        return Task.CompletedTask;
    }
}