using System.Threading.Tasks;
using Chatly.Contracts.SignalR;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR;

public abstract class NotificationStrategy<T>(ILogger<NotificationStrategy<T>> logger) : INotificationStrategy where T : NotificationMessage
{
    public abstract NotificationType Type { get; }
    public async Task HandleNotificationAsync(NotificationMessage message)
    {
        if(message is not T typedMessage)
        {
            logger.LogWarning("Received message of type {MessageType} but expected {ExpectedType}", message.GetType().Name, typeof(T).Name);
            return;
        }
        
        await HandleNotificationAsync(typedMessage);
        logger.LogInformation("Received message of type {MessageType} with {Message}", typeof(T).Name, typedMessage);
    }
    
    protected abstract Task HandleNotificationAsync(T message);
}