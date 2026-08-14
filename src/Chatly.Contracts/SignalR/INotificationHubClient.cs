using Chatly.Contracts.FriendRequests.Results;

namespace Chatly.Contracts.SignalR;

public interface INotificationClient
{
    Task Receive(NotificationMessage message);
}