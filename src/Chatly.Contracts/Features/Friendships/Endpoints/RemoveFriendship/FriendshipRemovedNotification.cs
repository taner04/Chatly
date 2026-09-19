namespace Chatly.Contracts.Features.Friendships.Endpoints.RemoveFriendship;

public sealed class FriendshipRemovedNotification(Guid associatedUserId)
    : Notification
{
    public Guid AssociatedUserId { get; } = associatedUserId;
}