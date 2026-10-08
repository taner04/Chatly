using Chatly.Contracts.Features.Users.Endpoints.UpdateUsername;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateUsername;

internal sealed class UpdateUsernameEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(
                ApiRoutes.Users.Username,
                async (
                        [FromBody] UpdateUsernameRequest request,
                        [FromServices] IMediator mediator,
                        CancellationToken cancellationToken) =>
                    Results.Ok(await mediator.Send(
                        new UpdateUsernameCommand(request.NewUsername),
                        cancellationToken)))
            .WithName("UpdateUsername")
            .WithTags("User")
            .RequireAuthorization()
            .Accepts<UpdateUsernameRequest>("application/json")
            .Produces<CurrentUserResponse>()
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict);
    }
}