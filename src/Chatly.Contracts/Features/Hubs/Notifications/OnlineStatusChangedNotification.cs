namespace Chatly.Contracts.Features.Hubs.Notifications;

public sealed class OnlineStatusChangedNotification(
    Guid userId,
    bool isOnline) : Notification
{
    public Guid UserId { get; } = userId;

    public bool IsOnline { get; } = isOnline;
}