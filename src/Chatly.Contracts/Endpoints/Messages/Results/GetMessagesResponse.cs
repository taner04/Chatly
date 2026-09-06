namespace Chatly.Contracts.Endpoints.Messages.Results;

public sealed record GetMessagesResponse(
    IReadOnlyList<GetMessagesItem> Items,
    DateTimeOffset? NextBeforeSentAt,
    Guid? NextBeforeMessageId,
    bool HasMore);