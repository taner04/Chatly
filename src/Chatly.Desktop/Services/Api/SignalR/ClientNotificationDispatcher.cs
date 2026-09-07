using System.Linq;
using Chatly.Contracts.SignalR;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR;

[SingletonService]
public sealed partial class ClientNotificationDispatcher(
    ILogger<ClientNotificationDispatcher> logger,
    IEnumerable<IClientNotificationHandler> notificationHandlers)
{
    public async Task DispatchAsync(NotificationMessage message)
    {
        var handler = notificationHandlers.FirstOrDefault(candidate => candidate.Type == message.Type);

        if (handler is null)
        {
            LogStrategyNotFound(message.Type);
            return;
        }

        await handler.HandleNotificationAsync(message);
    }

    [LoggerMessage(LogLevel.Warning, "No strategy found for notification type: {NotificationType}")]
    private partial void LogStrategyNotFound(NotificationType notificationType);
}