using Chatly.WebApi.Features.DeviceSessions.Models;

namespace Chatly.WebApi.Features.DeviceSessions.Endpoints.RevokeDeviceSession;

internal sealed record RevokeDeviceSessionCommand(DeviceSessionId SessionId) : ICommand;