using Chatly.Contracts.Common.Pagination;
using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Contracts.Features.FriendRequests.Models;
using Chatly.Contracts.Features.Friendships.Models;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.Services.Api.Clients;

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

    internal async Task<WebClientResult<PaginationResult<FriendRequestContract>>> GetFriendRequestsAsync(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.GetFriendRequestsAsync(pageIndex, pageSize, cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult<IReadOnlyList<FriendshipContract>>> GetFriendshipsAsync(
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

    internal async Task<WebClientResult<FriendshipContract>> AcceptFriendRequestAsync(
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