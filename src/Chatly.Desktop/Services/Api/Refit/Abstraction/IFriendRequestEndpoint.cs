using Chatly.Contracts.Endpoints.FriendRequests.Requests;
using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.Pagination;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Abstraction;

public interface IFriendRequestEndpoint
{
    [Get("/api/friend-requests")]
    Task<ApiResponse<PaginationResult<GetFriendRequestsResponse>>> GetFriendRequestsAsync(
        [AliasAs("pageIndex")] int pageIndex,
        [AliasAs("pageSize")] int pageSize,
        CancellationToken cancellationToken);

    [Post("/api/friend-requests")]
    Task<IApiResponse> SendFriendRequestAsync(
        [Body] SendFriendRequestRequest request,
        CancellationToken cancellationToken);

    [Post("/api/friend-requests/{friendRequestId}/accept")]
    Task<ApiResponse<AcceptFriendRequestResponse>> AcceptFriendRequestAsync(
        Guid friendRequestId,
        CancellationToken cancellationToken);

    [Post("/api/friend-requests/{friendRequestId}/reject")]
    Task<IApiResponse> RejectFriendRequestAsync(
        Guid friendRequestId,
        CancellationToken cancellationToken);
}