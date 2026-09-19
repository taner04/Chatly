namespace Chatly.Contracts.Features.Hubs.Abstraction;

public interface INotificationHubClient
{
    Task Receive(Notification notification);
}