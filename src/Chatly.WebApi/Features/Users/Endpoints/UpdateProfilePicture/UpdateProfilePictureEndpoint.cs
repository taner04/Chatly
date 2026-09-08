namespace Chatly.WebApi.Features.Users.Endpoints.UpdateProfilePicture;

internal sealed class UpdateProfilePictureEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(
                "/api/users/me/profile-picture",
                async (
                    [FromForm] IFormFile? file,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(
                        new UpdateProfilePictureCommand(file),
                        cancellationToken));
                })
            .WithName("UpdateProfilePicture")
            .WithTags("User")
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<CurrentUserResponse>()
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound);
    }
}
