using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.Pagination;
using Chatly.WebApi.Common.Infrastructure.Pagination;
using Chatly.WebApi.Features.FriendRequests.Enums;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.GetFriendRequests;

internal sealed class GetFriendRequestsQueryHandler(
    ChatlyDbContext context,
    CurrentUserService currentUserService,
    AzureBlobService blobService) : IQueryHandler<GetFriendRequestsQuery, PaginationResult<GetFriendRequestsResponse>>
{
    public async ValueTask<PaginationResult<GetFriendRequestsResponse>> Handle(
        GetFriendRequestsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var page = await context.FriendRequests
            .AsNoTracking()
            .Where(request => request.ReceiverUserId == userId)
            .Where(request => request.Status == FriendRequestStatus.Pending)
            .Where(request => request.SenderUser.Username != null)
            .OrderByDescending(request => request.CreatedAt)
            .ThenByDescending(request => request.Id)
            .Select(request => new
            {
                RequestId = request.Id.Value,
                request.SenderUser.Username,
                request.SenderUser.ProfilePictureKey
            })
            .ToPaginationResultAsync(query, cancellationToken);

        return page.Map(request => new GetFriendRequestsResponse(
            request.RequestId,
            request.Username!,
            blobService.CreateReadUrl(request.ProfilePictureKey)?.ToString()));
    }
}
