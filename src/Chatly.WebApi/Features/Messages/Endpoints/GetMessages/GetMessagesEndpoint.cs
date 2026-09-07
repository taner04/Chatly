using Chatly.Contracts.Endpoints.Messages.Results;
using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.Messages.Models;

namespace Chatly.WebApi.Features.Messages.Endpoints.GetMessages;

public sealed class GetMessagesEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(
                "/api/chats/{chatId:guid}/messages",
                async (
                    Guid chatId,
                    [FromQuery] DateTimeOffset? beforeSentAt,
                    [FromQuery] Guid? beforeMessageId,
                    [FromQuery] int pageSize,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetMessagesQuery(
                        ChatId.From(chatId),
                        beforeSentAt,
                        beforeMessageId.HasValue ? MessageId.From(beforeMessageId.Value) : null,
                        pageSize);

                    return Results.Ok(await mediator.Send(query, cancellationToken));
                })
            .WithName("GetMessages")
            .WithTags("Messages")
            .RequireAuthorization()
            .Produces<GetMessagesResponse>()
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound);
    }
}