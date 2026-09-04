using Chatly.Contracts.Endpoints.Chats.Results;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit;

public interface IChatEndpoint
{
    [Get("/api/chats")]
    Task<ApiResponse<IReadOnlyList<GetChatsResponse>>> GetChatsAsync(
        CancellationToken cancellationToken);
}
