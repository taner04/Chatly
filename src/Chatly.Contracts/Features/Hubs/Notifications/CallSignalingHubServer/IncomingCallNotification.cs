namespace Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;

public sealed record IncomingCallNotification(
    Guid CallId,
    Guid RemoteUserId,
    string? RemoteUsername,
    CallRole Role,
    CallState State) : Call(CallId, RemoteUserId, RemoteUsername, Role, State);
