using Chatly.Contracts.Features.Friendships.Models;

namespace Chatly.WebApi.Features.Friendships.Endpoints.GetFriendships;

internal sealed class GetFriendshipsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(
                ApiRoutes.Friendships.Collection,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                    Results.Ok(await mediator.Send(
                        new GetFriendshipsQuery(),
                        cancellationToken)))
            .WithName("GetFriendships")
            .WithTags("Friendships")
            .RequireAuthorization()
            .Produces<IReadOnlyList<FriendshipContract>>()
            .ProducesStandardErrors();
    }
}