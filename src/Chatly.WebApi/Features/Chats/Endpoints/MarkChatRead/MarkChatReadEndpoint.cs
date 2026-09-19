namespace Chatly.WebApi.Features.Chats.Endpoints.MarkChatRead;

internal sealed class MarkChatReadEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(
                ApiRoutes.Chats.Read,
                async (
                    [FromRoute] Guid chatId,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await mediator.Send(new MarkChatReadCommand(ChatId.From(chatId)), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("MarkChatRead")
            .WithTags("Chats")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors();
    }
}