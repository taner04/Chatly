using Chatly.Contracts.Endpoints.Messages.Requests;
using Chatly.Contracts.Endpoints.Messages.Results;
using Chatly.Desktop.Services.Api.Refit.Abstraction;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.Services.Api;

[TransientService]
public sealed class MessageApiClient(IChatlyApi chatlyApi)
{
    public async Task<WebClientResult<GetMessagesResponse>> GetMessagesAsync(
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

    public async Task<WebClientResult<SendMessageResponse>> SendMessageAsync(
        SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.SendMessageAsync(request, cancellationToken),
            cancellationToken);
    }
}