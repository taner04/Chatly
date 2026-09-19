namespace Chatly.WebApi.Features.Messages.Endpoints.RemoveMessage;

internal sealed class RemoveMessageEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(
                ApiRoutes.Messages.ById,
                async (
                    [FromRoute] Guid messageId,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await mediator.Send(
                        new RemoveMessageCommand(MessageId.From(messageId)),
                        cancellationToken);
                    return Results.NoContent();
                })
            .WithName("RemoveMessage")
            .WithTags("Messages")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound);
    }
}