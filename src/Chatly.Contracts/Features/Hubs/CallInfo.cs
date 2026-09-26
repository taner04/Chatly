namespace Chatly.Contracts.Features.Hubs;

public sealed record CallInfo(
    Guid CallId,
    Guid RemoteUserId,
    string? RemoteUsername,
    CallRole Role,
    CallState State,
    CallEndReason? EndReason,
    DateTimeOffset? AcceptedAt = null);