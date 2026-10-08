using Chatly.Contracts.Features.DeviceSessions.Models;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.Services.Api.Clients;

[TransientService]
public sealed class DeviceSessionApiClient(IChatlyApi chatlyApi)
{
    internal async Task<WebClientResult<IReadOnlyList<DeviceSessionContract>>> GetDeviceSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.GetDeviceSessionsAsync(cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult> RevokeDeviceSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.RevokeDeviceSessionAsync(sessionId, cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult> RevokeOtherDeviceSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.RevokeOtherDeviceSessionsAsync(cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult> RevokeCurrentDeviceSessionAsync(
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.RevokeCurrentDeviceSessionAsync(cancellationToken),
            cancellationToken);
    }
}