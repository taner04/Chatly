using System.Text.Json.Serialization;
using Chatly.Contracts.FriendRequests.Results;

namespace Chatly.Contracts.SignalR;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$notificationType")]
[JsonDerivedType(typeof(FriendRequestResponse), "IncomingFriendRequest")]
public abstract class NotificationMessage(NotificationType type)
{
    public NotificationType Type { get; } = type;
}