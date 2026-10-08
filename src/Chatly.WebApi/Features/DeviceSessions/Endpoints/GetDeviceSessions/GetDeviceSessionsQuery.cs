using Chatly.Contracts.Features.DeviceSessions.Models;

namespace Chatly.WebApi.Features.DeviceSessions.Endpoints.GetDeviceSessions;

internal sealed record GetDeviceSessionsQuery : IQuery<IReadOnlyList<DeviceSessionContract>>;