using Chatly.Contracts.Common.Pagination;
using Chatly.Contracts.Features.FriendRequests.Models;
using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.Users.Services.ProfilePictures;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.GetFriendRequests;

internal sealed class GetFriendRequestsQueryHandler(
    ChatlyDbContext context,
    CurrentUserService currentUserService,
    ProfilePictureUrlFactory profilePictureUrlFactory)
    : IQueryHandler<GetFriendRequestsQuery, PaginationResult<FriendRequestContract>>
{
    public async ValueTask<PaginationResult<FriendRequestContract>> Handle(
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
                SenderUserId = request.SenderUserId.Value,
                request.SenderUser.Username,
                ProfilePictureKey = request.SenderUser.ProfilePictureFile == null
                    ? null
                    : request.SenderUser.ProfilePictureFile.BlobName
            })
            .ToPaginationResultAsync(query, cancellationToken);

        return page.Map(request => new FriendRequestContract(
            request.RequestId,
            request.SenderUserId,
            request.Username!,
            profilePictureUrlFactory.CreateProfilePictureUrl(request.ProfilePictureKey)));
    }
}