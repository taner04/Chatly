namespace Chatly.Contracts.Features.Hubs.Abstraction;

public interface INotificationHubClient : IHubClient
{
    Task Receive(Notification notification);
}