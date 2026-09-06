using Chatly.Contracts.SignalR;

namespace Chatly.Desktop.Services.Api.SignalR.Abstraction;

public interface IClientNotificationHandler
{
    NotificationType Type { get; }

    Task HandleNotificationAsync(NotificationMessage message);
}