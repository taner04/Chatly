using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Chatly.ServiceDefaults;
using Chatly.WebApi.Features.Users.Models;

namespace Chatly.WebApi.Common.Infrastructure;

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
            logger.LogInformation("Azure Blob Storage initialized successfully.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to initialize Azure Blob Storage.");
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

    internal Uri CreateReadUrl(string blobName, TimeSpan lifetime)
    {
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

    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "Successfully uploaded blob {blobName}.")]
    private partial void LogUploadSucceeded(string blobName);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Failed to upload blob {blobName}.")]
    private partial void LogUploadFailed(string blobName, Exception exception);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Successfully deleted blob {blobName}.")]
    private partial void LogDeleteSucceeded(string blobName);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Blob {blobName} was not found during deletion.")]
    private partial void LogDeleteNotFound(string blobName);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Failed to delete blob {blobName}.")]
    private partial void LogDeleteFailed(string blobName, Exception exception);

    internal readonly record struct BlobUploadResult(
        string BlobName,
        bool Success,
        string? Error)
    {
        internal static BlobUploadResult Succeeded(string blobName) =>
            new(blobName, true, null);

        internal static BlobUploadResult Failed(string blobName, string error) =>
            new(blobName, false, error);
    }
}