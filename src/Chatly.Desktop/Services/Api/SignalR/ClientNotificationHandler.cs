using Chatly.Contracts.Features.Hubs;

namespace Chatly.Desktop.Services.Api.SignalR;

internal abstract partial class ClientNotificationHandler<TNotification>(
    ILogger<ClientNotificationHandler<TNotification>> logger)
    : IClientNotificationHandler where TNotification : Notification
{
    public Type NotificationType => typeof(TNotification);

    public async Task HandleNotificationAsync(Notification message)
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