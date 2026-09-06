namespace Chatly.Contracts.Endpoints.Messages.Results;

public sealed record SendMessageResponse(
    Guid MessageId,
    Guid ChatId,
    Guid SenderUserId,
    string Content,
    DateTimeOffset SentAt);