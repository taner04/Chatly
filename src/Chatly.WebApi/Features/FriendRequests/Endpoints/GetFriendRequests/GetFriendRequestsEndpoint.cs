using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.Pagination;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.GetFriendRequests;

public sealed class GetFriendRequestsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(
                "/api/friend-requests",
                async (
                    [FromQuery] int pageIndex,
                    [FromQuery] int pageSize,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetFriendRequestsQuery(pageIndex, pageSize);

                    return Results.Ok(await mediator.Send(query, cancellationToken));
                })
            .WithName("GetFriendRequests")
            .WithTags("Friend Request")
            .RequireAuthorization()
            .Produces<PaginationResult<GetFriendRequestsResponse>>()
            .ProducesStandardErrors(StatusCodes.Status400BadRequest);
    }
}