namespace Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;

public sealed record CallAcceptedNotification(
    Guid CallId,
    Guid RemoteUserId,
    string? RemoteUsername,
    CallRole Role,
    CallState State,
    DateTimeOffset AcceptedAt) : Call(CallId, RemoteUserId, RemoteUsername, Role, State);