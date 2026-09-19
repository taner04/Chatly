namespace Chatly.Contracts.Features.FriendRequests.Models;

public sealed record FriendRequestContract(
    Guid FriendRequestId,
    Guid SenderUserId,
    string SenderUsername,
    string? SenderProfilePictureUrl);