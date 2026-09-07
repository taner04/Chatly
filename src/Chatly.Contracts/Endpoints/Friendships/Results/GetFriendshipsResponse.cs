namespace Chatly.Contracts.Endpoints.Friendships.Results;

public sealed record GetFriendshipsResponse(
    Guid FriendshipId,
    Guid? DirectChatId,
    Guid FriendUserId,
    string FriendUsername,
    string? FriendProfilePictureUrl,
    bool IsOnline);