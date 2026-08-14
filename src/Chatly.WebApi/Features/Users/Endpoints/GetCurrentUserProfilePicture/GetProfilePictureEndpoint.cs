using Chatly.Contracts.Users.Results;
using Chatly.WebApi.Common.Abstractions;
using Chatly.WebApi.Common.Extensions;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Chatly.WebApi.Features.Users.Endpoints.GetProfilePicture;

public sealed class GetProfilePictureEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(
                "/api/users/me/profile-picture",
                async (
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetCurrentUserProfilePictureQuery();
                    return Results.Ok(
                        await mediator.Send(query, cancellationToken));
                })
            .WithName("GetCurrentUserProfilePicture")
            .WithTags("User")
            .RequireAuthorization()
            .Produces<GetPictureResponse>()
            .ProducesStandardErrors();
    }
}
