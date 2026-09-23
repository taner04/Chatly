using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Chatly.Contracts.Common.Policies;
using Chatly.ServiceDefaults;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Blob;

[SingletonService]
internal sealed partial class AzureBlobService(
    ILogger<AzureBlobService> logger,
    BlobServiceClient blobServiceClient)
{
    private BlobContainerClient _containerClient = null!;

    internal async Task InitializeAsync()
    {
        try
        {
            _containerClient = blobServiceClient.GetBlobContainerClient(
                AppHostConstants.ProfilePicturesContainerName);
            await _containerClient.CreateIfNotExistsAsync();
            LogInitializationSucceeded();
        }
        catch (Exception exception)
        {
            LogInitializationFailed(exception);
            throw;
        }
    }

    internal async Task<bool> IsReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _containerClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            LogReadinessCheckFailed(exception);
            return false;
        }
    }

    internal async Task<BlobUploadResult> UploadAsync(
        string contentType,
        Stream stream,
        UserId userId,
        CancellationToken cancellationToken)
    {
        var blobName = CreateProfilePictureBlobName(userId, contentType);

        return await UploadAsync(contentType, stream, blobName, cancellationToken);
    }

    internal async Task<BlobUploadResult> UploadFileAsync(
        string contentType,
        Stream stream,
        UserId userId,
        string fileName,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var blobName = $"users/{userId}/files/{Guid.CreateVersion7():N}{extension}";

        return await UploadAsync(contentType, stream, blobName, cancellationToken);
    }

    private async Task<BlobUploadResult> UploadAsync(
        string contentType,
        Stream stream,
        string blobName,
        CancellationToken cancellationToken)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                },
                cancellationToken);

            LogUploadSucceeded(blobName);
            return BlobUploadResult.Succeeded(blobName);
        }
        catch (Exception exception)
        {
            LogUploadFailed(blobName, exception);
            return BlobUploadResult.Failed(blobName, exception.Message);
        }
    }

    internal async Task DeleteAsync(string blobName,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _containerClient
                .GetBlobClient(blobName)
                .DeleteIfExistsAsync(cancellationToken: cancellationToken);

            if (response.Value)
            {
                LogDeleteSucceeded(blobName);
            }
            else
            {
                LogDeleteNotFound(blobName);
            }
        }
        catch (Exception exception)
        {
            LogDeleteFailed(blobName, exception);
        }
    }

    internal Uri? CreateReadUrl(string? blobName) => CreateReadUrl(blobName, TimeSpan.FromMinutes(15));

    private Uri? CreateReadUrl(string? blobName, TimeSpan lifetime)
    {
        if (string.IsNullOrEmpty(blobName))
        {
            return null;
        }

        var blobClient = _containerClient.GetBlobClient(blobName);
        return blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(lifetime));
    }

    private static string CreateProfilePictureBlobName(UserId userId, string contentType)
    {
        var extension = contentType switch
        {
            ImageContentTypePolicy.Jpeg => ".jpg",
            ImageContentTypePolicy.Png => ".png",
            ImageContentTypePolicy.WebP => ".webp",
            _ => throw new InvalidOperationException(
                $"Unsupported content type: {contentType}")
        };

        return $"users/{userId}/profile/{Guid.CreateVersion7():N}{extension}";
    }
}