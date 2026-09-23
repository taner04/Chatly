namespace Chatly.Contracts.Features.Hubs.Abstraction;

public interface INotificationHubServer
{
    Task StartTyping(Guid chatId);
    Task StopTyping(Guid chatId);
}