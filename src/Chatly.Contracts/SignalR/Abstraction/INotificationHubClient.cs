namespace Chatly.Contracts.SignalR.Abstraction;

public interface INotificationHubClient
{
    Task Receive(NotificationMessage message);
}