using Chatly.Contracts.Pagination;
using Chatly.Contracts.Users.Results;
using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Common.Infrastructure.Pagination;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

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
                user.Username.StartsWith(query.SearchName))
            .OrderBy(user => user.Username)
            .ThenBy(user => user.Id)
            .Select(user => new
            {
                UserId = user.Id.Value,
                user.Username,
                user.ProfilePictureKey
            })
            .ToPaginationResultAsync(query, cancellationToken);

        return page.Map(user => new UserSearchResponse(
            user.UserId,
            user.Username!,
            blobService.CreateReadUrl(user.ProfilePictureKey)));
    }
}
