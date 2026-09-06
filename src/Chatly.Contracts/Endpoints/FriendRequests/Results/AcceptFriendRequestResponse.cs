namespace Chatly.Contracts.Endpoints.FriendRequests.Results;

public sealed record AcceptFriendRequestResponse(
    Guid FriendshipId,
    Guid DirectChatId,
    Guid FriendUserId,
    string FriendUsername,
    string? FriendProfilePictureUrl,
    bool IsOnline);