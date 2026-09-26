using Chatly.Contracts.Common.Pagination;
using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.Users.Services.ProfilePictures;

namespace Chatly.WebApi.Features.Users.Endpoints.SearchUsers;

internal sealed class SearchUsersQueryHandler(
    ChatlyDbContext context,
    CurrentUserService currentUser,
    ProfilePictureUrlFactory profilePictureUrlFactory)
    : IQueryHandler<SearchUsersQuery, PaginationResult<UserSearchResponse>>
{
    private const string LikeEscapeCharacter = "\\";

    public async ValueTask<PaginationResult<UserSearchResponse>> Handle(
        SearchUsersQuery query,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.GetCurrentUserId();
        var pattern = $"%{EscapeLikePattern(query.SearchName)}%";

        var page = await context.Users
            .AsNoTracking()
            .Where(user =>
                user.Id != currentUserId &&
                user.Username != null &&
                EF.Functions.ILike(user.Username, pattern, LikeEscapeCharacter))
            .OrderBy(user => user.Username)
            .ThenBy(user => user.Id)
            .Select(user => new
            {
                UserId = user.Id.Value,
                user.Username,
                ProfilePictureKey = user.ProfilePictureFile == null
                    ? null
                    : user.ProfilePictureFile.BlobName,
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
            profilePictureUrlFactory.CreateProfilePictureUrl(user.ProfilePictureKey),
            user.RelationshipStatus));
    }

    private static string EscapeLikePattern(string value) =>
        value
            .Replace(LikeEscapeCharacter, LikeEscapeCharacter + LikeEscapeCharacter, StringComparison.Ordinal)
            .Replace("%", LikeEscapeCharacter + "%", StringComparison.Ordinal)
            .Replace("_", LikeEscapeCharacter + "_", StringComparison.Ordinal);
}
