using Chatly.WebApi.Features.DeviceSessions.Models;

namespace Chatly.WebApi.Features.DeviceSessions.Endpoints.RevokeDeviceSession;

internal sealed class RevokeDeviceSessionEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(
                ApiRoutes.Users.SessionById,
                async (
                    [FromRoute] Guid sessionId,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await mediator.Send(
                        new RevokeDeviceSessionCommand(DeviceSessionId.From(sessionId)),
                        cancellationToken);

                    return Results.NoContent();
                })
            .WithName("RevokeDeviceSession")
            .WithTags("DeviceSessions")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict);
    }
}