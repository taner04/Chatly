using Avalonia.Platform.Storage;
using Chatly.Contracts.Features.StoredFiles.Endpoints.UploadAttachment;
using Chatly.Desktop.Services.Api.Multipart;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.Services.Api.Clients;

[TransientService]
public sealed class AttachmentApiClient(IChatlyApi chatlyApi)
{
    internal async Task<WebClientResult<UploadAttachmentResponse>> UploadAsync(
        IStorageFile file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        await using var parts = await MultipartStreamPartFactory.OpenOwnedAsync([file]);

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.UploadAttachmentAsync(parts.Single, cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult> RemoveAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.RemoveAttachmentAsync(attachmentId, cancellationToken),
            cancellationToken);
    }
}