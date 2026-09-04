namespace Chatly.Contracts.SignalR;

public interface IWebApiNotificationClient
{
    Task Receive(NotificationMessage message);
}