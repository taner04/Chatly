using System.IO;
using Avalonia.Platform.Storage;
using Chatly.Contracts.Common.Policies;
using Chatly.Contracts.Features.Messages.Endpoints.GetMessages;
using Chatly.Contracts.Features.Messages.Endpoints.SendMessage;
using Chatly.Contracts.Features.Messages.Models;
using Chatly.Desktop.Services.Api.Multipart;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.Services.Api.Clients;

[TransientService]
public sealed class MessageApiClient(IChatlyApi chatlyApi)
{
    internal async Task<WebClientResult<GetMessagesResponse>> GetMessagesAsync(
        GetMessagesRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.GetMessagesAsync(
                request.ChatId,
                request.BeforeSentAt,
                request.BeforeMessageId,
                request.PageSize,
                cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult<MessageContract>> SendMessageAsync(
        SendMessageRequest request,
        IReadOnlyCollection<IStorageFile> files,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(files);

        await using var parts = await MultipartStreamPartFactory.OpenOwnedAsync(files, GetContentType);

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.SendMessageAsync(
                request.ChatId,
                request.Content,
                parts.Parts,
                cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult> RemoveMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.RemoveMessageAsync(messageId, cancellationToken),
            cancellationToken);
    }

    private static string GetContentType(string fileName) =>
        ImageContentTypePolicy.FromFileExtension(Path.GetExtension(fileName))
        ?? "application/octet-stream";
}