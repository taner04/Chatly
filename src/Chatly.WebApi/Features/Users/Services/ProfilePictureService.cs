namespace Chatly.WebApi.Features.Users.Services;

[ScopedService]
internal sealed class ProfilePictureService(AzureBlobService blobService)
{
    internal async Task<ProfilePictureChange> PrepareReplacementAsync(
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

    internal ProfilePictureChange PrepareRemoval(User user)
    {
        var previousBlobName = user.ProfilePictureKey;
        user.ProfilePictureKey = null;
        return new ProfilePictureChange(null, previousBlobName);
    }

    internal async Task CompleteAsync(
        ProfilePictureChange change,
        CancellationToken cancellationToken)
    {
        if (change.PreviousBlobName is not null)
        {
            await blobService.DeleteAsync(change.PreviousBlobName, cancellationToken);
        }
    }

    internal async Task RollbackAsync(ProfilePictureChange change)
    {
        if (change.NewBlobName is not null)
        {
            await blobService.DeleteAsync(change.NewBlobName, CancellationToken.None);
        }
    }

    internal readonly record struct ProfilePictureChange(
        string? NewBlobName,
        string? PreviousBlobName);
}
