using Chatly.Contracts.Features.Messages.Endpoints.GetMessages;
using Chatly.Contracts.Features.Messages.Models;
using Chatly.Contracts.Features.Reactions.Models;
using Chatly.WebApi.Common.Infrastructure.Persistence.Blob;
using Chatly.WebApi.Features.Chats.Services;

namespace Chatly.WebApi.Features.Messages.Endpoints.GetMessages;

internal sealed class GetMessagesQueryHandler(
    CurrentUserService currentUserService,
    ChatlyDbContext context,
    ChatAccessService chatAccessService,
    AzureBlobService blobService) : IQueryHandler<GetMessagesQuery, GetMessagesResponse>
{
    public async ValueTask<GetMessagesResponse> Handle(
        GetMessagesQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (await chatAccessService.GetAsync(query.ChatId, userId, cancellationToken) is null)
        {
            throw new EntityNotFoundException<Chat>(query.ChatId.Value);
        }

        var messagesQuery = context.Messages
            .AsNoTracking()
            .Where(message => message.ChatId == query.ChatId);

        if (query is { BeforeSentAt: { } beforeSentAt, BeforeMessageId: { } beforeMessageId })
        {
            // Npgsql translates ValueTuple.Create here to a PostgreSQL row-value comparison.
            // ReSharper disable EntityFramework.UnsupportedServerSideFunctionCall
            messagesQuery = messagesQuery.Where(message => EF.Functions.LessThan(
                ValueTuple.Create(message.SentAt, message.Id),
                ValueTuple.Create(beforeSentAt, beforeMessageId)));
            // ReSharper restore EntityFramework.UnsupportedServerSideFunctionCall
        }

        var messageRows = await messagesQuery
            .OrderByDescending(message => message.SentAt)
            .ThenByDescending(message => message.Id)
            .Take(query.PageSize + 1)
            .Select(message => new
            {
                MessageId = message.Id.Value,
                ChatId = message.ChatId.Value,
                SenderUserId = message.SenderUserId.Value,
                Content = message.IsDeleted ? string.Empty : message.Content,
                message.SentAt,
                message.IsDeleted,
                Attachments = message.Attachments
                    .Select(attachment => new
                    {
                        AttachmentId = attachment.StoredFile.Id.Value,
                        attachment.StoredFile.FileName,
                        attachment.StoredFile.ContentType,
                        attachment.StoredFile.Size,
                        attachment.StoredFile.BlobName
                    })
                    .ToList(),
                Reactions = message.Reactions
                    .Select(reaction => new MessageReactionContract(
                        reaction.Id.Value,
                        reaction.UserId.Value,
                        reaction.Type))
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var hasMore = messageRows.Count > query.PageSize;
        if (hasMore)
        {
            messageRows.RemoveAt(messageRows.Count - 1);
        }

        var oldestMessage = messageRows.LastOrDefault();
        messageRows.Reverse();

        var messages = messageRows
            .Select(message => new MessageContract(
                message.MessageId,
                message.ChatId,
                message.SenderUserId,
                message.Content,
                message.SentAt,
                message.IsDeleted,
                [
                    .. message.Attachments
                        .Select(attachment => new MessageAttachmentContract(
                            attachment.AttachmentId,
                            attachment.FileName,
                            attachment.ContentType,
                            attachment.Size,
                            blobService.CreateReadUrl(attachment.BlobName)!.ToString()))
                ],
                message.Reactions))
            .ToList();

        return new GetMessagesResponse(
            messages,
            oldestMessage?.SentAt,
            oldestMessage?.MessageId,
            hasMore);
    }
}