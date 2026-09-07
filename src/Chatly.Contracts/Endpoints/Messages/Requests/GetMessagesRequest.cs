namespace Chatly.Contracts.Endpoints.Messages.Requests;

public sealed record GetMessagesRequest(
    Guid ChatId,
    DateTimeOffset? BeforeSentAt = null,
    Guid? BeforeMessageId = null,
    int PageSize = 50);