using Chatly.Contracts.Features.Hubs;

namespace Chatly.Desktop.Services.Api.SignalR;

[SingletonService]
internal sealed partial class ClientNotificationDispatcher
{
    private readonly ILogger<ClientNotificationDispatcher> _logger;
    private readonly IReadOnlyDictionary<Type, IClientNotificationHandler> _notificationHandlers;

    public ClientNotificationDispatcher(
        ILogger<ClientNotificationDispatcher> logger,
        IEnumerable<IClientNotificationHandler> notificationHandlers)
    {
        _logger = logger;
        _notificationHandlers = notificationHandlers.ToDictionary(handler => handler.NotificationType);
    }

    public async Task DispatchAsync(Notification notification)
    {
        var notificationType = notification.GetType();
        if (!_notificationHandlers.TryGetValue(notificationType, out var handler))
        {
            LogStrategyNotFound(notificationType);
            return;
        }

        await handler.HandleNotificationAsync(notification);
    }

    [LoggerMessage(LogLevel.Warning, "No strategy found for notification type: {NotificationType}")]
    private partial void LogStrategyNotFound(Type notificationType);
}