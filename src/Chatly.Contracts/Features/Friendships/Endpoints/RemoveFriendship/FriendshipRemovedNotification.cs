namespace Chatly.Contracts.Features.Friendships.Endpoints.RemoveFriendship;

public sealed class FriendshipRemovedNotification(Guid associatedUserId)
    : NotificationMessage
{
    public Guid AssociatedUserId { get; } = associatedUserId;
}