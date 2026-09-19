using Chatly.Contracts.Common.Pagination;

namespace Chatly.Contracts.Features.Messages.Endpoints.GetMessages;

public sealed record GetMessagesRequest(
    Guid ChatId,
    DateTimeOffset? BeforeSentAt = null,
    Guid? BeforeMessageId = null,
    int PageSize = PaginationPolicy.MessagesPageSize);