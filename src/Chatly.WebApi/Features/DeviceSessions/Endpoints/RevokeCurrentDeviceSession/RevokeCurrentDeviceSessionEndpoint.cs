namespace Chatly.WebApi.Features.DeviceSessions.Endpoints.RevokeCurrentDeviceSession;

internal sealed class RevokeCurrentDeviceSessionEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(
                ApiRoutes.Users.CurrentSession,
                async (
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await mediator.Send(
                        new RevokeCurrentDeviceSessionCommand(),
                        cancellationToken);

                    return Results.NoContent();
                })
            .WithName("RevokeCurrentDeviceSession")
            .WithTags("DeviceSessions")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors(StatusCodes.Status400BadRequest);
    }
}