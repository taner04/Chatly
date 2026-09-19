using Chatly.Contracts.Features.Messages.Endpoints.SendMessage;
using Chatly.Contracts.Features.Messages.Models;
using Chatly.WebApi.Common.Infrastructure.Persistence.Blob;
using Chatly.WebApi.Features.Chats.Services;
using Chatly.WebApi.Features.MessageAttachments.Models;
using Chatly.WebApi.Features.StoredFiles.Models;
using Chatly.WebApi.Features.StoredFiles.Services;

namespace Chatly.WebApi.Features.Messages.Endpoints.SendMessage;

internal sealed class SendMessageCommandHandler(
    CurrentUserService currentUserService,
    ChatlyDbContext context,
    AzureBlobService blobService,
    ChatAccessService chatAccessService,
    StoredFileService storedFileService,
    NotificationPublisher notificationPublisher) : ICommandHandler<SendMessageCommand, MessageContract>
{
    public async ValueTask<MessageContract> Handle(
        SendMessageCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var chat = await chatAccessService.GetAsync(command.ChatId, userId, cancellationToken)
                   ?? throw new EntityNotFoundException<Chat>(command.ChatId.Value);

        var message = new Message(chat.ChatId, userId, command.Content?.Trim() ?? string.Empty);
        context.Messages.Add(message);

        var uploadedBlobNames = new List<string>();
        var attachments = new List<StoredFile>();

        try
        {
            foreach (var file in command.Files)
            {
                await using var content = file.OpenReadStream();
                var storedFile = await storedFileService.UploadAttachmentAsync(
                    userId,
                    content,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    cancellationToken);
                uploadedBlobNames.Add(storedFile.BlobName);
                context.MessageAttachments.Add(new MessageAttachment(message.Id, storedFile.Id));
                attachments.Add(storedFile);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await storedFileService.RollbackAsync(uploadedBlobNames);
            throw;
        }

        var attachmentResponses = attachments
            .Select(file => new MessageAttachmentContract(
                file.Id.Value,
                file.FileName,
                file.ContentType,
                file.Size,
                blobService.CreateReadUrl(file.BlobName)!.ToString()))
            .ToList();

        var contract = new MessageContract(
            message.Id.Value,
            message.ChatId.Value,
            message.SenderUserId.Value,
            message.Content,
            message.SentAt,
            false,
            attachmentResponses,
            []);

        await notificationPublisher.PublishAsync(
            chat.OtherParticipantUserId,
            new IncomingMessageNotification(contract));

        return contract;
    }
}