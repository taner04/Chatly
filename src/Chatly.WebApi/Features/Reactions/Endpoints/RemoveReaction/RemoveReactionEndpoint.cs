using Chatly.WebApi.Features.Reactions.Models;

namespace Chatly.WebApi.Features.Reactions.Endpoints.RemoveReaction;

internal sealed class RemoveReactionEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(
                ApiRoutes.Reactions.ById,
                async (
                    [FromRoute] Guid reactionId,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await mediator.Send(
                        new RemoveReactionCommand(ReactionId.From(reactionId)),
                        cancellationToken);

                    return Results.NoContent();
                })
            .WithName("RemoveReaction")
            .WithTags("Reactions")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound);
    }
}