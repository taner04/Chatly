using Chatly.Contracts.Features.DeviceSessions.Models;

namespace Chatly.WebApi.Features.DeviceSessions.Endpoints.GetDeviceSessions;

internal sealed class GetDeviceSessionsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(
                ApiRoutes.Users.Sessions,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                    Results.Ok(await mediator.Send(
                        new GetDeviceSessionsQuery(),
                        cancellationToken)))
            .WithName("GetDeviceSessions")
            .WithTags("DeviceSessions")
            .RequireAuthorization()
            .Produces<IReadOnlyList<DeviceSessionContract>>()
            .ProducesStandardErrors();
    }
}