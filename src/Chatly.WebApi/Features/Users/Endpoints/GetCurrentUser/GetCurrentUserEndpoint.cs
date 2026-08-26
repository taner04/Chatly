using Chatly.WebApi.Common.Abstractions;
using Chatly.WebApi.Common.Extensions;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Chatly.WebApi.Features.Users.Endpoints.GetCurrentUser;

public sealed class GetCurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/users/me",
                async ([FromServices] IMediator mediator, CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(new GetCurrentUserQuery(), cancellationToken));
                })
            .WithName("GetCurrentUser")
            .WithTags("User")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .ProducesStandardErrors(StatusCodes.Status404NotFound);
    }
}