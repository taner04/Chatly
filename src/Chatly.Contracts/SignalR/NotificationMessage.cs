using Chatly.Contracts.FriendRequests.Results;
using System.Text.Json.Serialization;

namespace Chatly.Contracts.SignalR;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$notificationType")]
[JsonDerivedType(typeof(FriendRequestResponse), "FriendRequestReceived")]
public abstract class NotificationMessage(NotificationType type)
{
    public NotificationType Type { get; } = type;
}
