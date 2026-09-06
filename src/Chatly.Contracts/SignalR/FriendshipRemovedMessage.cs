namespace Chatly.Contracts.SignalR;

public sealed class FriendshipRemovedMessage(Guid associatedUserId)
    : NotificationMessage(NotificationType.FriendshipRemoved)
{
    public Guid AssociatedUserId { get; } = associatedUserId;
}