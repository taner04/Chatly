using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.Pagination;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.GetFriendRequests;

internal sealed record GetFriendRequestsQuery(int PageIndex, int PageSize)
    : PaginationQuery(PageIndex, PageSize), IQuery<PaginationResult<GetFriendRequestsResponse>>;
