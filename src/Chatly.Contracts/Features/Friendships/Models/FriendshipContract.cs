namespace Chatly.Contracts.Features.Friendships.Models;

public sealed record FriendshipContract(
    Guid FriendshipId,
    Guid DirectChatId,
    Guid FriendUserId,
    string FriendUsername,
    string? FriendProfilePictureUrl,
    bool IsOnline);