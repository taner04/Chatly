using Chatly.Contracts.Features.DeviceSessions.Models;
using Chatly.WebApi.Features.DeviceSessions.Services;

namespace Chatly.WebApi.Features.DeviceSessions.Endpoints.GetDeviceSessions;

internal sealed class GetDeviceSessionsQueryHandler(DeviceSessionService deviceSessionService)
    : IQueryHandler<GetDeviceSessionsQuery, IReadOnlyList<DeviceSessionContract>>
{
    public async ValueTask<IReadOnlyList<DeviceSessionContract>> Handle(
        GetDeviceSessionsQuery query,
        CancellationToken cancellationToken)
    {
        var sessions = await deviceSessionService.GetActiveAsync(cancellationToken);

        return [.. sessions.Select(deviceSessionService.CreateContract)];
    }
}