namespace Chatly.Contracts.SignalR;

public sealed class OnlineStatusChangedMessage(
    Guid userId,
    bool isOnline) : NotificationMessage(NotificationType.OnlineStatusChanged)
{
    public Guid UserId { get; } = userId;

    public bool IsOnline { get; } = isOnline;
}