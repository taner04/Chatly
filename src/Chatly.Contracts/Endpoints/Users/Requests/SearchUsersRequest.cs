using Chatly.Contracts.Pagination;

namespace Chatly.Contracts.Endpoints.Users.Requests;

public sealed record SearchUsersRequest(
    string SearchName,
    int PageIndex = 1,
    int PageSize = 20) : PaginationQuery(PageIndex, PageSize);