using Chatly.WebApi.Common.Infrastructure.Blob;
using Chatly.WebApi.Features.StoredFiles.Models;

namespace Chatly.WebApi.Features.StoredFiles.Endpoints.RemoveAttachment;

internal sealed class RemoveAttachmentCommandHandler(
    CurrentUserService currentUserService,
    AzureBlobService blobService,
    ChatlyDbContext context) : ICommandHandler<RemoveAttachmentCommand>
{
    public async ValueTask<Unit> Handle(
        RemoveAttachmentCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        var storedFile = await context.StoredFiles.SingleOrDefaultAsync(
            file => file.Id == command.StoredFileId &&
                    file.UserId == userId &&
                    !context.MessageAttachments.Any(attachment => attachment.StoredFileId == file.Id),
            cancellationToken) ?? throw new EntityNotFoundException<StoredFile>(command.StoredFileId.Value);

        context.StoredFiles.Remove(storedFile);
        await context.SaveChangesAsync(cancellationToken);
        await blobService.DeleteAsync(storedFile.BlobName, cancellationToken);
        return Unit.Value;
    }
}