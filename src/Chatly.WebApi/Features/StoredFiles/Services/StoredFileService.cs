using Chatly.WebApi.Common.Infrastructure.Blob;
using Chatly.WebApi.Features.StoredFiles.Models;

namespace Chatly.WebApi.Features.StoredFiles.Services;

[ScopedService]
internal sealed class StoredFileService(
    AzureBlobService blobService,
    ChatlyDbContext context)
{
    internal async Task<StoredFile> UploadAttachmentAsync(
        UserId userId,
        Stream content,
        string fileName,
        string contentType,
        long size,
        CancellationToken cancellationToken)
    {
        var upload = await blobService.UploadFileAsync(
            contentType,
            content,
            userId,
            fileName,
            cancellationToken);

        return await AddStoredFileAsync(
            upload,
            userId,
            fileName,
            contentType,
            size,
            "Attachment");
    }

    internal async Task<StoredFile> UploadProfilePictureAsync(
        UserId userId,
        Stream content,
        string fileName,
        string contentType,
        long size,
        CancellationToken cancellationToken)
    {
        var upload = await blobService.UploadAsync(contentType, content, userId, cancellationToken);

        return await AddStoredFileAsync(
            upload,
            userId,
            fileName,
            contentType,
            size,
            "Profile picture");
    }

    internal Task DeleteAsync(string blobName, CancellationToken cancellationToken) =>
        blobService.DeleteAsync(blobName, cancellationToken);

    internal async Task RollbackAsync(IEnumerable<string> blobNames)
    {
        foreach (var blobName in blobNames)
        {
            await blobService.DeleteAsync(blobName, CancellationToken.None);
        }
    }

    private async Task<StoredFile> AddStoredFileAsync(
        BlobUploadResult upload,
        UserId userId,
        string fileName,
        string contentType,
        long size,
        string uploadType)
    {
        if (!upload.Success)
        {
            throw new InvalidOperationException(
                $"{uploadType} upload failed: {upload.Error ?? "Unknown error."}");
        }

        try
        {
            var storedFile = new StoredFile(
                userId,
                upload.BlobName,
                fileName,
                contentType,
                size);
            context.StoredFiles.Add(storedFile);
            return storedFile;
        }
        catch
        {
            await blobService.DeleteAsync(upload.BlobName, CancellationToken.None);
            throw;
        }
    }
}