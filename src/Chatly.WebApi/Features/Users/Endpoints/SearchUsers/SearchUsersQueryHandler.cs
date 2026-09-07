using Chatly.Contracts.Pagination;
using Chatly.WebApi.Common.Infrastructure.Pagination;
using Chatly.WebApi.Features.FriendRequests.Enums;

namespace Chatly.WebApi.Features.Users.Endpoints.SearchUsers;

public sealed class SearchUsersQueryHandler(
    ChatlyDbContext context,
    CurrentUserService currentUser,
    AzureBlobService blobService)
    : IQueryHandler<SearchUsersQuery, PaginationResult<UserSearchResponse>>
{
    public async ValueTask<PaginationResult<UserSearchResponse>> Handle(
        SearchUsersQuery query,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.GetCurrentUserId();

        var page = await context.Users
            .AsNoTracking()
            .Where(user =>
                user.Id != currentUserId &&
                user.Username != null &&
                EF.Functions.ILike(user.Username, $"%{query.SearchName}%"))
            .OrderBy(user => user.Username)
            .ThenBy(user => user.Id)
            .Select(user => new
            {
                UserId = user.Id.Value,
                user.Username,
                user.ProfilePictureKey,
                RelationshipStatus = context.Friendships.Any(friendship =>
                    (friendship.FirstUserId == currentUserId && friendship.SecondUserId == user.Id) ||
                    (friendship.SecondUserId == currentUserId && friendship.FirstUserId == user.Id))
                    ? UserRelationshipStatus.Friends
                    : context.FriendRequests
                        .Where(request =>
                            request.Status == FriendRequestStatus.Pending &&
                            ((request.SenderUserId == currentUserId && request.ReceiverUserId == user.Id) ||
                             (request.ReceiverUserId == currentUserId && request.SenderUserId == user.Id)))
                        .Select(request => request.SenderUserId == currentUserId
                            ? UserRelationshipStatus.OutgoingFriendRequest
                            : UserRelationshipStatus.IncomingFriendRequest)
                        .FirstOrDefault()
            })
            .ToPaginationResultAsync(query, cancellationToken);

        return page.Map(user => new UserSearchResponse(
            user.UserId,
            user.Username!,
            blobService.CreateReadUrl(user.ProfilePictureKey),
            user.RelationshipStatus));
    }
}