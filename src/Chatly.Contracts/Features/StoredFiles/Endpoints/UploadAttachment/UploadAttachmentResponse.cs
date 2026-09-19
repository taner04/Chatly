namespace Chatly.Contracts.Features.StoredFiles.Endpoints.UploadAttachment;

public sealed record UploadAttachmentResponse(
    Guid AttachmentId,
    string FileName,
    string ContentType,
    long Size);