namespace Chatly.WebApi.Common.Infrastructure.Blob;

internal readonly record struct BlobUploadResult(
    string BlobName,
    bool Success,
    string? Error)
{
    internal static BlobUploadResult Succeeded(string blobName) => new(blobName, true, null);

    internal static BlobUploadResult Failed(string blobName, string error) => new(blobName, false, error);
}