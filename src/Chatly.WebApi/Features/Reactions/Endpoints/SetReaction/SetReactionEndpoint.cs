using Chatly.Contracts.Features.Reactions.Endpoints.SetReaction;
using Chatly.Contracts.Features.Reactions.Models;

namespace Chatly.WebApi.Features.Reactions.Endpoints.SetReaction;

internal sealed class SetReactionEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(
                ApiRoutes.Messages.Reaction,
                async (
                    [FromRoute] Guid messageId,
                    [FromBody] SetReactionRequest request,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(
                        new SetReactionCommand(MessageId.From(messageId), request.ReactionType),
                        cancellationToken));
                })
            .WithName("SetReaction")
            .WithTags("Reactions")
            .RequireAuthorization()
            .Accepts<SetReactionRequest>("application/json")
            .Produces<MessageReactionContract>()
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound);
    }
}