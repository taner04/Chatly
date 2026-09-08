using Chatly.Contracts.Endpoints.FriendRequests.Results;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.AcceptFriendRequest;

internal sealed class AcceptFriendRequestEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(
                "/api/friend-requests/{friendRequestId:guid}/accept",
                async (
                    [FromRoute] Guid friendRequestId,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(
                        new AcceptFriendRequestCommand(friendRequestId),
                        cancellationToken));
                })
            .WithName("AcceptFriendRequest")
            .WithTags("Friend Request")
            .RequireAuthorization()
            .Produces<AcceptFriendRequestResponse>()
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict);
    }
}
