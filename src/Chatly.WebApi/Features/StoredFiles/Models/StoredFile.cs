namespace Chatly.WebApi.Features.StoredFiles.Models;

[ValueObject<Guid>]
public readonly partial struct StoredFileId
{
    private static Validation Validate(Guid value) => value.Validate<StoredFileId>();
}

public sealed class StoredFile : Entity<StoredFileId>
{
    internal const int MaxBlobNameLength = 1_024;
    internal const int MaxFileNameLength = 255;
    internal const int MaxContentTypeLength = 255;

    private StoredFile()
    {
    }

    public StoredFile(
        UserId userId,
        string blobName,
        string fileName,
        string contentType,
        long size)
        : base(StoredFileId.From(Guid.CreateVersion7()))
    {
        UserId = userId;
        BlobName = blobName;
        FileName = fileName;
        ContentType = contentType;
        Size = size;
    }

    public UserId UserId { get; private init; }

    public string BlobName { get; private init; } = string.Empty;

    public string FileName { get; private init; } = string.Empty;

    public string ContentType { get; private init; } = string.Empty;

    public long Size { get; private init; }
}