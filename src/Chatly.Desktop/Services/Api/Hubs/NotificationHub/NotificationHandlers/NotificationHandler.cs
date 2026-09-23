using Chatly.Desktop.Abstraction.Hubs;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

internal abstract partial class NotificationHandler<TNotification>(
    ILogger<NotificationHandler<TNotification>> logger)
    : IHubMessageHandler<Notification> where TNotification : Notification
{
    public Type MessageType => typeof(TNotification);

    public async Task HandleAsync(Notification message)
    {
        if (message is not TNotification typedMessage)
        {
            LogUnexpectedMessageType(message.GetType().Name, typeof(TNotification).Name);
            return;
        }

        await HandleNotificationAsync(typedMessage);
        LogMessageHandled(typeof(TNotification).Name, typedMessage);
    }

    protected abstract Task HandleNotificationAsync(TNotification message);

    [LoggerMessage(LogLevel.Warning, "Received message of type {MessageType} but expected {ExpectedType}")]
    private partial void LogUnexpectedMessageType(string messageType, string expectedType);

    [LoggerMessage(LogLevel.Information, "Received message of type {MessageType} with {Message}")]
    private partial void LogMessageHandled(string messageType, TNotification message);
}