using Chatly.Contracts.Features.Messages.Models;

namespace Chatly.Contracts.Features.Messages.Endpoints.GetMessages;

public sealed record GetMessagesResponse(
    IReadOnlyList<MessageContract> Items,
    DateTimeOffset? NextBeforeSentAt,
    Guid? NextBeforeMessageId,
    bool HasMore);