using Chatly.Contracts.Features.Chats.Endpoints.GetChats;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Endpoints;

public interface IChatEndpoint
{
    [Get(ApiRoutes.Chats.Collection)]
    Task<ApiResponse<IReadOnlyList<GetChatsResponse>>> GetChatsAsync(
        CancellationToken cancellationToken);

    [Put(ApiRoutes.Chats.Read)]
    Task<IApiResponse> MarkChatReadAsync(
        Guid chatId,
        CancellationToken cancellationToken);
}