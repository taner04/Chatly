namespace Chatly.WebApi.Features.Users.Endpoints.CompleteOnboarding;

internal sealed class CompleteOnboardingEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(
                "/api/users/me/onboarding",
                async (
                    [FromForm] string newUsername,
                    [FromForm] IFormFile? file,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await using var content = file?.OpenReadStream();
                    var command = new CompleteOnboardingCommand(
                        newUsername,
                        content,
                        file?.FileName,
                        file?.ContentType,
                        file?.Length ?? 0);

                    return Results.Ok(await mediator.Send(command, cancellationToken));
                })
            .WithName("CompleteOnboarding")
            .WithTags("User")
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<CurrentUserResponse>()
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict);
    }
}
