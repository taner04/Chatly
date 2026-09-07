using Chatly.Contracts.Endpoints.Friendships.Results;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Abstraction;

public interface IFriendshipEndpoint
{
    [Get("/api/friendships")]
    Task<ApiResponse<IReadOnlyList<GetFriendshipsResponse>>> GetFriendshipsAsync(
        CancellationToken cancellationToken);

    [Delete("/api/friendships/{associatedUserId}")]
    Task<IApiResponse> RemoveFriendshipAsync(
        Guid associatedUserId,
        CancellationToken cancellationToken);
}