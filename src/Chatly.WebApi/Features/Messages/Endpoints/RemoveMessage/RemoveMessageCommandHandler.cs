using Chatly.Contracts.Features.Messages.Endpoints.RemoveMessage;
using Chatly.WebApi.Common.Infrastructure.Persistence.Blob;

namespace Chatly.WebApi.Features.Messages.Endpoints.RemoveMessage;

internal sealed class RemoveMessageCommandHandler(
    ChatlyDbContext context,
    CurrentUserService userService,
    AzureBlobService blobService,
    NotificationPublisher notificationPublisher) : ICommandHandler<RemoveMessageCommand>
{
    public async ValueTask<Unit> Handle(RemoveMessageCommand command, CancellationToken cancellationToken)
    {
        var userId = userService.GetCurrentUserId();

        var message = await context.Messages
                          .Include(message => message.Chat)
                          .Include(message => message.Reactions)
                          .Include(message => message.Attachments)
                          .ThenInclude(attachment => attachment.StoredFile)
                          .Where(message => message.Id == command.MessageId &&
                                            message.SenderUserId == userId &&
                                            !message.IsDeleted)
                          .FirstOrDefaultAsync(cancellationToken) ??
                      throw new EntityNotFoundException<Message>(command.MessageId.Value);

        var storedFiles = message.Attachments
            .Select(attachment => attachment.StoredFile)
            .ToList();
        var blobNames = storedFiles
            .Select(file => file.BlobName)
            .ToList();

        message.Attachments.Clear();
        message.Reactions.Clear();
        context.StoredFiles.RemoveRange(storedFiles);
        message.Content = string.Empty;
        message.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        foreach (var blobName in blobNames)
        {
            await blobService.DeleteAsync(blobName, cancellationToken);
        }

        await notificationPublisher.PublishAsync(
            [message.Chat.FirstUserId, message.Chat.SecondUserId],
            new MessageDeletedNotification(message.ChatId.Value, message.Id.Value));

        return Unit.Value;
    }
}