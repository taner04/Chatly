using Chatly.Contracts.Endpoints.FriendRequests.Requests;
using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.Endpoints.Friendships.Results;
using Chatly.Contracts.Pagination;
using Chatly.Desktop.Services.Api.Refit.Abstraction;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.Services.Api;

[TransientService]
public sealed class FriendsApiClient(IChatlyApi chatlyApi)
{
    internal async Task<WebClientResult> SendFriendRequestAsync(
        SendFriendRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.SendFriendRequestAsync(request, cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult<PaginationResult<GetFriendRequestsResponse>>> GetFriendRequestsAsync(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.GetFriendRequestsAsync(pageIndex, pageSize, cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult<IReadOnlyList<GetFriendshipsResponse>>> GetFriendshipsAsync(
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.GetFriendshipsAsync(cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult> RemoveFriendshipAsync(
        Guid associatedUserId,
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.RemoveFriendshipAsync(associatedUserId, cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult<AcceptFriendRequestResponse>> AcceptFriendRequestAsync(
        Guid friendRequestId,
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.AcceptFriendRequestAsync(friendRequestId, cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult> RejectFriendRequestAsync(
        Guid friendRequestId,
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.RejectFriendRequestAsync(friendRequestId, cancellationToken),
            cancellationToken);
    }
}
