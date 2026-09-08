using Chatly.Contracts.Pagination;

namespace Chatly.WebApi.Features.Users.Endpoints.SearchUsers;

internal sealed record SearchUsersQuery(string SearchName, int PageIndex, int PageSize)
    : PaginationQuery(PageIndex, PageSize), IQuery<PaginationResult<UserSearchResponse>>;
