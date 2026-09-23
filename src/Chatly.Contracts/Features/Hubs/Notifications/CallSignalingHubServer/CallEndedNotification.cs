namespace Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;

public sealed record CallEndedNotification(
    Guid CallId,
    Guid RemoteUserId,
    string? RemoteUsername,
    CallRole Role,
    CallState State,
    CallEndReason Reason) : Call(CallId, RemoteUserId, RemoteUsername, Role, State);
