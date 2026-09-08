using Chatly.Contracts.SignalR;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR;

internal abstract partial class ClientNotificationHandler<TMessage>(ILogger<ClientNotificationHandler<TMessage>> logger)
    : IClientNotificationHandler where TMessage : NotificationMessage
{
    public abstract NotificationType Type { get; }

    public async Task HandleNotificationAsync(NotificationMessage message)
    {
        if (message is not TMessage typedMessage)
        {
            LogUnexpectedMessageType(message.GetType().Name, typeof(TMessage).Name);
            return;
        }

        await HandleNotificationAsync(typedMessage);
        LogMessageHandled(typeof(TMessage).Name, typedMessage);
    }

    protected abstract Task HandleNotificationAsync(TMessage message);

    [LoggerMessage(LogLevel.Warning, "Received message of type {MessageType} but expected {ExpectedType}")]
    private partial void LogUnexpectedMessageType(string messageType, string expectedType);

    [LoggerMessage(LogLevel.Information, "Received message of type {MessageType} with {Message}")]
    private partial void LogMessageHandled(string messageType, TMessage message);
}
