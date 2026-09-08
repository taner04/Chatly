using Chatly.Contracts.Endpoints.Chats.Results;

namespace Chatly.WebApi.Features.Chats.Endpoints.GetChats;

internal sealed class GetChatsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(
                "/api/chats",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                    Results.Ok(await mediator.Send(new GetChatsQuery(), cancellationToken)))
            .WithName("GetChats")
            .WithTags("Chats")
            .RequireAuthorization()
            .Produces<IReadOnlyList<GetChatsResponse>>()
            .ProducesStandardErrors();
    }
}
