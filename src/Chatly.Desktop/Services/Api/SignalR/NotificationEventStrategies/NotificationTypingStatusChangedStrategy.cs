using Chatly.Contracts.SignalR;
using Chatly.Desktop.Utilities;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationEventStrategies;

[SingletonService(typeof(INotificationStrategy))]
public sealed class NotificationTypingStatusChangedStrategy(
    ChatPageViewModel chatPage,
    ILogger<NotificationStrategy<TypingStatusChangedMessage>> logger)
    : NotificationStrategy<TypingStatusChangedMessage>(logger)
{
    public override NotificationType Type => NotificationType.TypingStatusChanged;

    protected override Task HandleNotificationAsync(TypingStatusChangedMessage message)
    {
        DispatcherUtility.SafeInvoke(() => chatPage.ReceiveTypingStatus(message));
        return Task.CompletedTask;
    }
}
