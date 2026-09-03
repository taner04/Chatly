using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Chatly.ServiceDefaults;

namespace Chatly.WebApi.Common.Infrastructure;

[SingletonService]
public sealed partial class AzureBlobService(
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

    internal async Task<BlobUploadResult> UploadAsync(
        string contentType,
        Stream stream,
        UserId userId,
        CancellationToken cancellationToken)
    {
        var blobName = CreateBlobName(userId, contentType);

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

    internal async Task<bool> DeleteAsync(
        string blobName,
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

            return response.Value;
        }
        catch (Exception exception)
        {
            LogDeleteFailed(blobName, exception);
            return false;
        }
    }

    internal Uri? CreateReadUrl(string? blobName)
    {
        return CreateReadUrl(blobName, TimeSpan.FromMinutes(15));
    }

    internal Uri? CreateReadUrl(string? blobName, TimeSpan lifetime)
    {
        if (string.IsNullOrEmpty(blobName))
        {
            return null;
        }

        var blobClient = _containerClient.GetBlobClient(blobName);
        return blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(lifetime));
    }

    private static string CreateBlobName(UserId userId, string contentType)
    {
        var extension = contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new InvalidOperationException(
                $"Unsupported content type: {contentType}")
        };

        return $"users/{userId}/profile/{Guid.CreateVersion7():N}{extension}";
    }

    internal readonly record struct BlobUploadResult(
        string BlobName,
        bool Success,
        string? Error)
    {
        internal static BlobUploadResult Succeeded(string blobName)
        {
            return new BlobUploadResult(blobName, true, null);
        }

        internal static BlobUploadResult Failed(string blobName, string error)
        {
            return new BlobUploadResult(blobName, false, error);
        }
    }
}