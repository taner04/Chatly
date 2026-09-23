namespace Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;

public sealed record IceCandidateNotification(
    Guid CallId,
    Guid RemoteUserId,
    string? RemoteUsername,
    CallRole Role,
    CallState State,
    string Candidate,
    string? SdpMid,
    int? SdpMLineIndex) : Call(CallId, RemoteUserId, RemoteUsername, Role, State);
