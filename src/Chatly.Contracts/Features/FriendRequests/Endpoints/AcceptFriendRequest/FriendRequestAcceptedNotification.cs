using Chatly.Contracts.Features.Friendships.Models;

namespace Chatly.Contracts.Features.FriendRequests.Endpoints.AcceptFriendRequest;

public sealed class FriendRequestAcceptedNotification(FriendshipContract friendship)
    : NotificationMessage
{
    public FriendshipContract Friendship { get; } = friendship;
}