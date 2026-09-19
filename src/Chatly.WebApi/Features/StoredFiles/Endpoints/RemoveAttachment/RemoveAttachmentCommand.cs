using Chatly.WebApi.Features.StoredFiles.Models;

namespace Chatly.WebApi.Features.StoredFiles.Endpoints.RemoveAttachment;

internal sealed record RemoveAttachmentCommand(StoredFileId StoredFileId) : ICommand;