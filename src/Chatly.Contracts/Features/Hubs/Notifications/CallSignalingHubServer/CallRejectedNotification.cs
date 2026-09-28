namespace Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;

public sealed record CallRejectedNotification(
    Guid CallId,
    Guid RemoteUserId,
    string? RemoteUsername,
    CallRole Role,
    CallState State,
    CallEndReason Reason) : CallMessage(CallId, RemoteUserId, RemoteUsername, Role, State);