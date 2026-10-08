using Chatly.WebApi.Features.DeviceSessions.Models;

namespace Chatly.WebApi.Features.Hubs;

internal static class HubGroups
{
    internal static string User(UserId userId) => $"user:{userId.Value}";

    internal static string DeviceSession(DeviceSessionId sessionId) => $"device-session:{sessionId.Value}";
}