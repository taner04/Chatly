namespace Chatly.WebApi.Features.Friendships.Endpoints.RemoveFriendship;

internal sealed class RemoveFriendshipEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(
                "/api/friendships/{associatedUserId:guid}",
                async (
                    [FromRoute] Guid associatedUserId,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await mediator.Send(
                        new RemoveFriendshipCommand(UserId.From(associatedUserId)),
                        cancellationToken);

                    return Results.NoContent();
                })
            .WithName("RemoveFriendship")
            .WithTags("Friendships")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound);
    }
}
