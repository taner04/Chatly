using Chatly.Contracts.Features.DeviceSessions.Models;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Endpoints;

public interface IDeviceSessionEndpoint
{
    [Get(ApiRoutes.Users.Sessions)]
    Task<ApiResponse<IReadOnlyList<DeviceSessionContract>>> GetDeviceSessionsAsync(
        CancellationToken cancellationToken);

    [Delete(ApiRoutes.Users.SessionById)]
    Task<IApiResponse> RevokeDeviceSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken);

    [Delete(ApiRoutes.Users.Sessions)]
    Task<IApiResponse> RevokeOtherDeviceSessionsAsync(
        CancellationToken cancellationToken);

    [Delete(ApiRoutes.Users.CurrentSession)]
    Task<IApiResponse> RevokeCurrentDeviceSessionAsync(
        CancellationToken cancellationToken);
}