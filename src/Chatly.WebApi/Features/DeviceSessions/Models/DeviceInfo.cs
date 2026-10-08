namespace Chatly.WebApi.Features.DeviceSessions.Models;

internal sealed record DeviceInfo(
    Guid DeviceId,
    string DeviceName,
    string Platform,
    string AppVersion);