using Chatly.Contracts.Features.Messages.Models;

namespace Chatly.WebApi.Features.Messages.Endpoints.SendMessage;

internal sealed class SendMessageEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(
                ApiRoutes.Messages.Collection,
                async (
                        [FromForm] Guid chatId,
                        [FromForm] string? content,
                        [FromForm] IFormFileCollection? files,
                        [FromServices] IMediator mediator,
                        CancellationToken cancellationToken) =>
                    Results.Ok(await mediator.Send(
                        new SendMessageCommand(
                            ChatId.From(chatId),
                            content,
                            files?.ToArray() ?? []),
                        cancellationToken)))
            .WithName("SendMessage")
            .WithTags("Messages")
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFileCollection>("multipart/form-data")
            .Produces<MessageContract>()
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound);
    }
}