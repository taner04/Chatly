using Chatly.Contracts.Endpoints.Messages.Results;
using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.Messages.Models;

namespace Chatly.WebApi.Features.Messages.Endpoints.GetMessages;

public sealed record GetMessagesQuery(
    ChatId ChatId,
    DateTimeOffset? BeforeSentAt,
    MessageId? BeforeMessageId,
    int PageSize) : IQuery<GetMessagesResponse>;