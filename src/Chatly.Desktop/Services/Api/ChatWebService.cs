using Chatly.Contracts.Endpoints.Chats.Results;
using Chatly.Desktop.Services.Api.Refit.Abstraction;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.Services.Api;

[TransientService]
public sealed class ChatWebService(IChatlyApi chatlyApi)
{
    public async Task<WebClientResult<IReadOnlyList<GetChatsResponse>>> GetChatsAsync(
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.GetChatsAsync(cancellationToken),
            cancellationToken);
    }
}