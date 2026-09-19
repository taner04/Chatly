namespace Chatly.Contracts.Features.Chats.Endpoints.GetChats;

public sealed record GetChatsResponse(
    Guid ChatId,
    Guid AssociatedUserId,
    string AssociatedUsername,
    string? AssociatedProfilePictureUrl,
    bool IsOnline,
    int UnreadMessageCount);