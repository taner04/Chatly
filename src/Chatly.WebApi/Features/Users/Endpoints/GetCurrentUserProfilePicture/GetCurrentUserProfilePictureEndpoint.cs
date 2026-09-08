namespace Chatly.WebApi.Features.Users.Endpoints.GetCurrentUserProfilePicture;

internal sealed class GetCurrentUserProfilePictureEndpoint : IEndpoint
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
            .Produces<GetCurrentUserProfilePictureResponse>()
            .ProducesStandardErrors();
    }
}
