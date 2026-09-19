using Chatly.WebApi.Common.Infrastructure.Persistence.Blob;

namespace Chatly.WebApi.Features.Users.Services.ProfilePictures;

[SingletonService]
internal sealed class ProfilePictureUrlFactory(AzureBlobService blobService)
{
    internal string? CreateProfilePictureUrl(string? profilePictureKey)
    {
        if (string.IsNullOrWhiteSpace(profilePictureKey))
        {
            return null;
        }

        return blobService.CreateReadUrl(profilePictureKey)?.ToString();
    }
}