using Chatly.Contracts.Features.Messages.Endpoints.GetMessages;

namespace Chatly.WebApi.Features.Messages.Endpoints.GetMessages;

internal sealed class GetMessagesEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(
                ApiRoutes.Chats.Messages,
                async (
                    [AsParameters] GetMessagesRequest request,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetMessagesQuery(
                        ChatId.From(request.ChatId),
                        request.BeforeSentAt,
                        request.BeforeMessageId.HasValue ? MessageId.From(request.BeforeMessageId.Value) : null,
                        request.PageSize);

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