using Chatly.Contracts.SignalR;

namespace Chatly.Desktop.Services.Api.SignalR.Abstraction;

internal interface IClientNotificationHandler
{
    NotificationType Type { get; }

    Task HandleNotificationAsync(NotificationMessage message);
}
