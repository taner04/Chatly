using Chatly.Contracts.Features.Chats.Endpoints.GetChats;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.Services.Api.Clients;

[TransientService]
public sealed class ChatApiClient(IChatlyApi chatlyApi)
{
    internal async Task<WebClientResult<IReadOnlyList<GetChatsResponse>>> GetChatsAsync(
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.GetChatsAsync(cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult> MarkChatReadAsync(
        Guid chatId,
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.MarkChatReadAsync(chatId, cancellationToken),
            cancellationToken);
    }
}