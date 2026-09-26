using System.Text.Json.Serialization;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;

namespace Chatly.Contracts.Features.Hubs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$callType")]
[JsonDerivedType(typeof(IncomingCallNotification), "IncomingCall")]
[JsonDerivedType(typeof(CallAcceptedNotification), "CallAccepted")]
[JsonDerivedType(typeof(CallRejectedNotification), "CallRejected")]
[JsonDerivedType(typeof(CallEndedNotification), "CallEnded")]
[JsonDerivedType(typeof(CallStateChangedNotification), "CallStateChanged")]
public abstract record Call(
    Guid CallId,
    Guid RemoteUserId,
    string? RemoteUsername,
    CallRole Role,
    CallState State) : IHubMessage;