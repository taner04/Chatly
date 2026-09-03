using Chatly.Contracts.Endpoints.Friendships.Results;

namespace Chatly.WebApi.Features.Friendships.Endpoints.GetFriendships;

public sealed class GetFriendshipsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(
                "/api/friendships",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(
                        new GetFriendshipsQuery(),
                        cancellationToken));
                })
            .WithName("GetFriendships")
            .WithTags("Friendships")
            .RequireAuthorization()
            .Produces<IReadOnlyList<GetFriendshipsResponse>>()
            .ProducesStandardErrors();
    }
}