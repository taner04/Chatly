using Chatly.Contracts.Common.Pagination;
using Chatly.Contracts.Features.FriendRequests.Models;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.GetFriendRequests;

internal sealed class GetFriendRequestsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(
                ApiRoutes.FriendRequests.Collection,
                async (
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken,
                    [FromQuery] int pageIndex = PaginationPolicy.DefaultPageIndex,
                    [FromQuery] int pageSize = PaginationPolicy.DefaultPageSize) =>
                {
                    var query = new GetFriendRequestsQuery(pageIndex, pageSize);

                    return Results.Ok(await mediator.Send(query, cancellationToken));
                })
            .WithName("GetFriendRequests")
            .WithTags("Friend Request")
            .RequireAuthorization()
            .Produces<PaginationResult<FriendRequestContract>>()
            .ProducesStandardErrors(StatusCodes.Status400BadRequest);
    }
}