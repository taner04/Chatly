namespace Chatly.WebApi.Features.Users.Endpoints.UpdateUsername;

public sealed class UpdateUsernameEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(
                "/api/users/me/username",
                async (
                    [FromBody] UpdateUsernameCommand command,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(command, cancellationToken));
                })
            .WithName("UpdateUsername")
            .WithTags("User")
            .RequireAuthorization()
            .Accepts<UpdateUsernameCommand>("application/json")
            .Produces<CurrentUserResponse>()
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict);
    }
}