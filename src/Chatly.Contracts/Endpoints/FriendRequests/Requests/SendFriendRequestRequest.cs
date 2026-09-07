namespace Chatly.Contracts.Endpoints.FriendRequests.Requests;

public sealed record SendFriendRequestRequest(Guid ReceiverId);