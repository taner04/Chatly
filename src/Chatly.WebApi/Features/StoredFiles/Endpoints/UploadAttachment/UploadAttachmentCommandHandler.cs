using Chatly.Contracts.Features.StoredFiles.Endpoints.UploadAttachment;
using Chatly.WebApi.Features.StoredFiles.Services;

namespace Chatly.WebApi.Features.StoredFiles.Endpoints.UploadAttachment;

internal sealed class UploadAttachmentCommandHandler(
    CurrentUserService currentUserService,
    StoredFileService storedFileService,
    ChatlyDbContext context) : ICommandHandler<UploadAttachmentCommand, UploadAttachmentResponse>
{
    public async ValueTask<UploadAttachmentResponse> Handle(
        UploadAttachmentCommand command,
        CancellationToken cancellationToken)
    {
        var file = command.File!;
        var userId = currentUserService.GetCurrentUserId();
        await using var content = file.OpenReadStream();
        var storedFile = await storedFileService.UploadAttachmentAsync(
            userId,
            content,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await storedFileService.RollbackAsync([storedFile.BlobName]);
            throw;
        }

        return new UploadAttachmentResponse(
            storedFile.Id.Value,
            storedFile.FileName,
            storedFile.ContentType,
            storedFile.Size);
    }
}