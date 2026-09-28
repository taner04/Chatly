using Chatly.Contracts.Features.FriendRequests.Models;

namespace Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;

public sealed class IncomingFriendRequestNotification(FriendRequestContract request) : NotificationMessage
{
    public FriendRequestContract Request { get; } = request;
}