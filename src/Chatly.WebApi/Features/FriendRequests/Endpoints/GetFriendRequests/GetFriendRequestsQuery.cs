using Chatly.Contracts.Common.Pagination;
using Chatly.Contracts.Features.FriendRequests.Models;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.GetFriendRequests;

internal sealed record GetFriendRequestsQuery(int PageIndex, int PageSize)
    : PaginationQuery(PageIndex, PageSize), IQuery<PaginationResult<FriendRequestContract>>;