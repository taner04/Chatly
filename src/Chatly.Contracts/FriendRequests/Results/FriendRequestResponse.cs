using Chatly.Contracts.SignalR;

namespace Chatly.Contracts.FriendRequests.Results;

public sealed class FriendRequestResponse(
    Guid friendRequestId,
    Guid senderUserId,
    string senderUsername,
    Uri? senderProfilePictureUrl) : NotificationMessage(NotificationType.FriendRequestReceived)
{
    public Guid FriendRequestId { get; } = friendRequestId;
    public Guid SenderUserId { get; } = senderUserId;
    public string SenderUsername { get; } = senderUsername;
    public Uri? SenderProfilePictureUrl { get; } = senderProfilePictureUrl;
}
