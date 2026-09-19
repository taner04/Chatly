namespace Chatly.Contracts.Features.Messages.Models;

public sealed record MessageAttachmentContract(
    Guid AttachmentId,
    string FileName,
    string ContentType,
    long Size,
    string Url);