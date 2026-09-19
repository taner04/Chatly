using Chatly.Contracts.Features.Friendships.Models;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Abstraction;

public interface IFriendshipEndpoint
{
    [Get(ApiRoutes.Friendships.Collection)]
    Task<ApiResponse<IReadOnlyList<FriendshipContract>>> GetFriendshipsAsync(
        CancellationToken cancellationToken);

    [Delete(ApiRoutes.Friendships.ByAssociatedUserId)]
    Task<IApiResponse> RemoveFriendshipAsync(
        Guid associatedUserId,
        CancellationToken cancellationToken);
}