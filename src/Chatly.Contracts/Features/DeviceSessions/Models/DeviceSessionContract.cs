namespace Chatly.Contracts.Features.DeviceSessions.Models;

public sealed record DeviceSessionContract(
    Guid SessionId,
    string DeviceName,
    string Platform,
    string AppVersion,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastSeenAt,
    bool IsCurrent);