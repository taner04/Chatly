namespace Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;

public sealed class OnlineStatusChangedNotification(
    Guid userId,
    bool isOnline) : NotificationMessage
{
    public Guid UserId { get; } = userId;

    public bool IsOnline { get; } = isOnline;
}