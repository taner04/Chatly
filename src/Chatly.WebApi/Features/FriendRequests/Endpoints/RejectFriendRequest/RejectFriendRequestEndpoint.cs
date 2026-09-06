namespace Chatly.WebApi.Features.FriendRequests.Endpoints.RejectFriendRequest;

public sealed class RejectFriendRequestEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(
                "/api/friend-requests/{friendRequestId:guid}/reject",
                async (
                    [FromRoute] Guid friendRequestId,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await mediator.Send(
                        new RejectFriendRequestCommand(friendRequestId),
                        cancellationToken);

                    return Results.NoContent();
                })
            .WithName("RejectFriendRequest")
            .WithTags("Friend Request")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict);
    }
}