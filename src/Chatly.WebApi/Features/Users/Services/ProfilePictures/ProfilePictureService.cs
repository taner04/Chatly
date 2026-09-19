using Chatly.WebApi.Features.StoredFiles.Services;

namespace Chatly.WebApi.Features.Users.Services.ProfilePictures;

[ScopedService]
internal sealed class ProfilePictureService(
    StoredFileService storedFileService,
    ChatlyDbContext context)
{
    internal async Task<ProfilePictureChange> PrepareReplacementAsync(
        User user,
        Stream content,
        string fileName,
        string contentType,
        long size,
        CancellationToken cancellationToken)
    {
        var previousFile = user.ProfilePictureFile;
        var storedFile = await storedFileService.UploadProfilePictureAsync(
            user.Id,
            content,
            fileName,
            contentType,
            size,
            cancellationToken);
        user.ProfilePictureFileId = storedFile.Id;
        user.ProfilePictureFile = storedFile;

        if (previousFile is not null)
        {
            context.StoredFiles.Remove(previousFile);
        }

        return new ProfilePictureChange(storedFile.BlobName, previousFile?.BlobName);
    }

    internal ProfilePictureChange PrepareRemoval(User user)
    {
        var previousFile = user.ProfilePictureFile;
        user.ProfilePictureFileId = null;
        user.ProfilePictureFile = null;

        if (previousFile is not null)
        {
            context.StoredFiles.Remove(previousFile);
        }

        return new ProfilePictureChange(null, previousFile?.BlobName);
    }

    internal async Task CompleteAsync(
        ProfilePictureChange change,
        CancellationToken cancellationToken)
    {
        if (change.PreviousBlobName is not null)
        {
            await storedFileService.DeleteAsync(change.PreviousBlobName, cancellationToken);
        }
    }

    internal async Task RollbackAsync(ProfilePictureChange change)
    {
        if (change.NewBlobName is not null)
        {
            await storedFileService.RollbackAsync([change.NewBlobName]);
        }
    }
}