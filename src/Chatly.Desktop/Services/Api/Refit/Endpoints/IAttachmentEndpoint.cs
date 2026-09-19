using Chatly.Contracts.Features.StoredFiles.Endpoints.UploadAttachment;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Abstraction;

public interface IAttachmentEndpoint
{
    [Multipart]
    [Post(ApiRoutes.Attachments.Collection)]
    Task<ApiResponse<UploadAttachmentResponse>> UploadAttachmentAsync(
        [AliasAs("file")] StreamPart file,
        CancellationToken cancellationToken);

    [Delete(ApiRoutes.Attachments.ById)]
    Task<IApiResponse> RemoveAttachmentAsync(
        Guid attachmentId,
        CancellationToken cancellationToken);
}