using Chatly.Contracts.Endpoints.Messages.Results;
using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.Messages.Models;

namespace Chatly.WebApi.Features.Messages.Endpoints.GetMessages;

internal sealed class GetMessagesQueryHandler(
    CurrentUserService currentUserService,
    ChatlyDbContext context) : IQueryHandler<GetMessagesQuery, GetMessagesResponse>
{
    public async ValueTask<GetMessagesResponse> Handle(
        GetMessagesQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        var hasAccess = await context.Chats
            .AsNoTracking()
            .AnyAsync(
                chat => chat.Id == query.ChatId &&
                        (chat.FirstUserId == userId || chat.SecondUserId == userId) &&
                        context.Friendships.Any(friendship =>
                            friendship.FirstUserId == chat.FirstUserId &&
                            friendship.SecondUserId == chat.SecondUserId),
                cancellationToken);

        if (!hasAccess)
        {
            throw new EntityNotFoundException<Chat>(query.ChatId.Value);
        }

        var messagesQuery = context.Messages
            .AsNoTracking()
            .Where(message => message.ChatId == query.ChatId);

        if (query.BeforeMessageId is { } beforeMessageId)
        {
            var beforeSentAt = await context.Messages
                                   .AsNoTracking()
                                   .Where(message => message.ChatId == query.ChatId && message.Id == beforeMessageId)
                                   .Select(message => (DateTimeOffset?)message.SentAt)
                                   .SingleOrDefaultAsync(cancellationToken)
                               ?? throw new EntityNotFoundException<Message>(beforeMessageId.Value);

            messagesQuery = messagesQuery.Where(message => message.SentAt < beforeSentAt);
        }

        var messages = await messagesQuery
            .OrderByDescending(message => message.SentAt)
            .ThenByDescending(message => message.Id)
            .Take(query.PageSize + 1)
            .Select(message => new GetMessagesItem(
                message.Id.Value,
                message.ChatId.Value,
                message.SenderUserId.Value,
                message.Content,
                message.SentAt))
            .ToListAsync(cancellationToken);

        var hasMore = messages.Count > query.PageSize;
        if (hasMore)
        {
            messages.RemoveAt(messages.Count - 1);
        }

        var oldestMessage = messages.LastOrDefault();
        messages.Reverse();

        return new GetMessagesResponse(
            messages,
            oldestMessage?.SentAt,
            oldestMessage?.MessageId,
            hasMore);
    }
}
