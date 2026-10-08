using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.FriendRequests.Models;

namespace Chatly.WebApi.Features.FriendRequests.Services;

[ScopedService]
internal sealed class PendingIncomingFriendRequestService(
    ChatlyDbContext context,
    CurrentUserService currentUserService)
{
    internal async Task<FriendRequest> GetAsync(
        Guid friendRequestId,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var requestId = FriendRequestId.From(friendRequestId);

        return await context.FriendRequests.FirstOrDefaultAsync(
                   request => request.Id == requestId &&
                              request.ReceiverUserId == userId &&
                              request.Status == FriendRequestStatus.Pending,
                   cancellationToken)
               ?? throw new EntityNotFoundException<FriendRequest>(friendRequestId);
    }
}