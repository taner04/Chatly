using Chatly.Contracts.Common.Pagination;

namespace Chatly.Contracts.Features.Users.Endpoints.SearchUsers;

public sealed record SearchUsersRequest(
    string SearchName,
    int PageIndex = PaginationPolicy.DefaultPageIndex,
    int PageSize = PaginationPolicy.DefaultPageSize) : PaginationQuery(PageIndex, PageSize);