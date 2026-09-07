using Chatly.Contracts.SignalR;

namespace Chatly.Contracts.Endpoints.FriendRequests.Results;

public sealed class IncomingFriendRequestMessage(
    Guid friendRequestId,
    Guid senderUserId,
    string senderUsername,
    Uri? senderProfilePictureUrl) : NotificationMessage(NotificationType.IncomingFriendRequest)
{
    public Guid FriendRequestId { get; } = friendRequestId;
    public Guid SenderUserId { get; } = senderUserId;
    public string SenderUsername { get; } = senderUsername;
    public Uri? SenderProfilePictureUrl { get; } = senderProfilePictureUrl;
}