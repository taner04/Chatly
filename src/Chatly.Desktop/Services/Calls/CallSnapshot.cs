namespace Chatly.Desktop.Services.Calls;

public sealed record CallSnapshot(
    Guid? CallId,
    Guid? RemoteUserId,
    string? RemoteUsername,
    CallState? State,
    bool IsIncoming,
    bool IsOnAnotherDevice)
{
    public static CallSnapshot Empty { get; } = new(null, null, null, null, false, false);

    public bool HasCall => CallId.HasValue;
}
