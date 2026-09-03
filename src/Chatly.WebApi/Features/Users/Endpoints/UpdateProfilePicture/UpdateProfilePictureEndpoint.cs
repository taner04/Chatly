namespace Chatly.WebApi.Features.Users.Endpoints.UpdateProfilePicture;

public sealed class UpdateProfilePictureEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(
                "/api/users/me/profile-picture",
                async (
                    [FromForm] IFormFile file,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await using var content = file.OpenReadStream();
                    var command = new UpdateProfilePictureCommand(
                        content,
                        file.FileName,
                        file.ContentType,
                        file.Length);

                    return Results.Ok(await mediator.Send(command, cancellationToken));
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