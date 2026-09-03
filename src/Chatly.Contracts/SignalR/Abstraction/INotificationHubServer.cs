namespace Chatly.Contracts.SignalR.Abstraction;

public interface INotificationHubServer
{
    Task IsOnline(Guid userId);
    Task IsOffline(Guid userId);
    Task StartTyping(Guid chatId);
    Task StopTyping(Guid chatId);
}