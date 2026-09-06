using Chatly.Contracts.SignalR;

namespace Chatly.Contracts.Endpoints.FriendRequests.Results;

public sealed class FriendRequestAcceptedMessage(
    Guid friendshipId,
    Guid directChatId,
    Guid friendUserId,
    string friendUsername,
    string? friendProfilePictureUrl,
    bool isOnline) : NotificationMessage(NotificationType.FriendRequestAccepted)
{
    public Guid FriendshipId { get; } = friendshipId;
    public Guid DirectChatId { get; } = directChatId;
    public Guid FriendUserId { get; } = friendUserId;
    public string FriendUsername { get; } = friendUsername;
    public string? FriendProfilePictureUrl { get; } = friendProfilePictureUrl;
    public bool IsOnline { get; } = isOnline;
}