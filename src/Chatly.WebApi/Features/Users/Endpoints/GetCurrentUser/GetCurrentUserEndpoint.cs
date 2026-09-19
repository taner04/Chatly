namespace Chatly.WebApi.Features.Users.Endpoints.GetCurrentUser;

internal sealed class GetCurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(ApiRoutes.Users.Current,
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