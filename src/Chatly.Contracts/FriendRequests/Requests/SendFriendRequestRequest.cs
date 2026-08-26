namespace Chatly.Contracts.FriendRequests.Requests;

public sealed record SendFriendRequestRequest(Guid ReceiverId);