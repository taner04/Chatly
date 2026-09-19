using Chatly.Contracts.Features.Hubs;

namespace Chatly.Desktop.Services.Api.SignalR.Abstraction;

internal interface IClientNotificationHandler
{
    Type NotificationType { get; }

    Task HandleNotificationAsync(Notification notification);
}