using Chatly.Contracts.Features.StoredFiles.Endpoints.UploadAttachment;

namespace Chatly.WebApi.Features.StoredFiles.Endpoints.UploadAttachment;

internal sealed record UploadAttachmentCommand(IFormFile? File) : ICommand<UploadAttachmentResponse>;