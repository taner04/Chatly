namespace Chatly.WebApi.Features.DeviceSessions.Models;

[ValueObject<Guid>]
public readonly partial struct DeviceSessionId : IGuidEntityId<DeviceSessionId>
{
    private static Validation Validate(Guid value) => value.Validate<DeviceSessionId>();
}

public sealed class DeviceSession : Entity<DeviceSessionId>
{
    internal const int MaxDeviceNameLength = 128;
    internal const int MaxPlatformLength = 64;
    internal const int MaxAppVersionLength = 32;
    internal const int MaxIdentitySessionIdLength = 128;

    [UsedImplicitly]
    private DeviceSession()
    {
    }

    public DeviceSession(
        UserId userId,
        Guid deviceId,
        string deviceName,
        string platform,
        string appVersion)
    {
        UserId = userId;
        DeviceId = deviceId;
        DeviceName = deviceName;
        Platform = platform;
        AppVersion = appVersion;
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    public UserId UserId { get; private init; }

    public Guid DeviceId { get; private init; }

    public string DeviceName { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public string AppVersion { get; set; } = string.Empty;

    public DateTimeOffset LastSeenAt { get; set; }

    public string? IdentitySessionId { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}