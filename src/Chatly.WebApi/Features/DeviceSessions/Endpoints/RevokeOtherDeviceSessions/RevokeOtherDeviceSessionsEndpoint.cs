namespace Chatly.WebApi.Features.DeviceSessions.Endpoints.RevokeOtherDeviceSessions;

internal sealed class RevokeOtherDeviceSessionsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(
                ApiRoutes.Users.Sessions,
                async (
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await mediator.Send(
                        new RevokeOtherDeviceSessionsCommand(),
                        cancellationToken);

                    return Results.NoContent();
                })
            .WithName("RevokeOtherDeviceSessions")
            .WithTags("DeviceSessions")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors(StatusCodes.Status400BadRequest);
    }
}