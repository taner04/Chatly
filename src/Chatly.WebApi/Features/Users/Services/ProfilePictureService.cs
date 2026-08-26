using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Features.Users.Models;

namespace Chatly.WebApi.Features.Users.Services;

public sealed class ProfilePictureService(AzureBlobService blobService)
{
    public async Task<ProfilePictureChange> PrepareReplacementAsync(
        User user,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        var previousBlobName = user.ProfilePictureKey;
        var upload = await blobService.UploadAsync(
            contentType,
            content,
            user.Id,
            cancellationToken);

        if (!upload.Success)
        {
            throw new InvalidOperationException(
                $"Profile picture upload failed: {upload.Error}");
        }

        user.ProfilePictureKey = upload.BlobName;
        return new ProfilePictureChange(upload.BlobName, previousBlobName);
    }

    public async Task CompleteAsync(
        ProfilePictureChange change,
        CancellationToken cancellationToken)
    {
        if (change.PreviousBlobName is not null)
        {
            await blobService.DeleteAsync(change.PreviousBlobName, cancellationToken);
        }
    }

    public async Task RollbackAsync(ProfilePictureChange change)
    {
        await blobService.DeleteAsync(change.NewBlobName, CancellationToken.None);
    }

    public readonly record struct ProfilePictureChange(
        string NewBlobName,
        string? PreviousBlobName);
}