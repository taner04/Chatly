using Chatly.Contracts.Common.Pagination;
using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Contracts.Features.FriendRequests.Models;
using Chatly.Contracts.Features.Friendships.Models;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Abstraction;

public interface IFriendRequestEndpoint
{
    [Get(ApiRoutes.FriendRequests.Collection)]
    Task<ApiResponse<PaginationResult<FriendRequestContract>>> GetFriendRequestsAsync(
        [AliasAs("pageIndex")] int pageIndex,
        [AliasAs("pageSize")] int pageSize,
        CancellationToken cancellationToken);

    [Post(ApiRoutes.FriendRequests.Collection)]
    Task<IApiResponse> SendFriendRequestAsync(
        [Body] SendFriendRequestRequest request,
        CancellationToken cancellationToken);

    [Post(ApiRoutes.FriendRequests.Accept)]
    Task<ApiResponse<FriendshipContract>> AcceptFriendRequestAsync(
        Guid friendRequestId,
        CancellationToken cancellationToken);

    [Post(ApiRoutes.FriendRequests.Reject)]
    Task<IApiResponse> RejectFriendRequestAsync(
        Guid friendRequestId,
        CancellationToken cancellationToken);
}