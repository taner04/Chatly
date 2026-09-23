namespace Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;

public sealed record WebRtcAnswerNotification(
    Guid CallId,
    Guid RemoteUserId,
    string? RemoteUsername,
    CallRole Role,
    CallState State,
    string Sdp) : Call(CallId, RemoteUserId, RemoteUsername, Role, State);
