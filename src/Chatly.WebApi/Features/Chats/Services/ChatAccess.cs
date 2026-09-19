namespace Chatly.WebApi.Features.Chats.Services;

internal sealed record ChatAccess(ChatId ChatId, UserId OtherParticipantUserId);