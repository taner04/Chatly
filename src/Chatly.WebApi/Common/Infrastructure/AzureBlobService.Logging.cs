namespace Chatly.WebApi.Common.Infrastructure;

public sealed partial class AzureBlobService
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "Successfully uploaded blob {BlobName}.")]
    private partial void LogUploadSucceeded(string blobName);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Failed to upload blob {BlobName}.")]
    private partial void LogUploadFailed(string blobName, Exception exception);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Successfully deleted blob {BlobName}.")]
    private partial void LogDeleteSucceeded(string blobName);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Blob {BlobName} was not found during deletion.")]
    private partial void LogDeleteNotFound(string blobName);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Failed to delete blob {BlobName}.")]
    private partial void LogDeleteFailed(string blobName, Exception exception);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Azure Blob Storage initialized successfully.")]
    private partial void LogInitializationSucceeded();

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Error,
        Message = "Failed to initialize Azure Blob Storage.")]
    private partial void LogInitializationFailed(Exception exception);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Warning,
        Message = "Azure Blob Storage readiness check failed.")]
    private partial void LogReadinessCheckFailed(Exception exception);
}