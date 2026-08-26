using Chatly.Contracts.Pagination;
using Chatly.Contracts.Users.Results;
using Mediator;

namespace Chatly.WebApi.Features.Users.Endpoints.SearchUsers;

public sealed record SearchUsersQuery(string SearchName, int PageIndex, int PageSize)
    : PaginationQuery(PageIndex, PageSize), IQuery<PaginationResult<UserSearchResponse>>;