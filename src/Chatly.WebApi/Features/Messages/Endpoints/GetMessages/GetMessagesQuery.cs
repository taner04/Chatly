using Chatly.Contracts.Features.Messages.Endpoints.GetMessages;

namespace Chatly.WebApi.Features.Messages.Endpoints.GetMessages;

internal sealed record GetMessagesQuery(
    ChatId ChatId,
    DateTimeOffset? BeforeSentAt,
    MessageId? BeforeMessageId,
    int PageSize) : IQuery<GetMessagesResponse>;