namespace Chatly.Contracts.Endpoints.Chats.Results;

public sealed record GetChatsResponse(
    Guid ChatId,
    Guid AssociatedUserId,
    string AssociatedUsername,
    string? AssociatedProfilePictureUrl,
    bool IsOnline);