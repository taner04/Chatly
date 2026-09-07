namespace Chatly.Contracts.Endpoints.FriendRequests.Results;

public sealed record GetFriendRequestsResponse(
    Guid FriendRequestId,
    string Username,
    string? ProfilePictureUrl);