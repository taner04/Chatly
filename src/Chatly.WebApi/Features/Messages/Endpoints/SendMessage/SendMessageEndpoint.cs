using Chatly.Contracts.Endpoints.Messages.Requests;
using Chatly.Contracts.Endpoints.Messages.Results;
using Chatly.WebApi.Features.Chats.Models;

namespace Chatly.WebApi.Features.Messages.Endpoints.SendMessage;

public sealed class SendMessageEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(
                "/api/messages",
                async (
                    [FromBody] SendMessageRequest request,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(
                        new SendMessageCommand(ChatId.From(request.ChatId), request.Content),
                        cancellationToken));
                })
            .WithName("SendMessage")
            .WithTags("Messages")
            .RequireAuthorization()
            .Accepts<SendMessageRequest>("application/json")
            .Produces<SendMessageResponse>()
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound);
    }
}