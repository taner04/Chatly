using Chatly.Contracts.Features.Friendships.Models;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.AcceptFriendRequest;

internal sealed class AcceptFriendRequestEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(
                ApiRoutes.FriendRequests.Accept,
                async (
                        [FromRoute] Guid friendRequestId,
                        [FromServices] IMediator mediator,
                        CancellationToken cancellationToken) =>
                    Results.Ok(await mediator.Send(
                        new AcceptFriendRequestCommand(friendRequestId),
                        cancellationToken)))
            .WithName("AcceptFriendRequest")
            .WithTags("Friend Request")
            .RequireAuthorization()
            .Produces<FriendshipContract>()
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict);
    }
}