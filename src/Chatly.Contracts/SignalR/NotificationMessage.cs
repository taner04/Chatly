using System.Text.Json.Serialization;
using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.Endpoints.Messages.Results;

namespace Chatly.Contracts.SignalR;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$notificationType")]
[JsonDerivedType(typeof(IncomingFriendRequestMessage), "IncomingFriendRequest")]
[JsonDerivedType(typeof(FriendRequestAcceptedMessage), "FriendRequestAccepted")]
[JsonDerivedType(typeof(IncomingChatMessage), "IncomingMessage")]
[JsonDerivedType(typeof(TypingStatusChangedMessage), "TypingStatusChanged")]
[JsonDerivedType(typeof(OnlineStatusChangedMessage), "OnlineStatusChanged")]
[JsonDerivedType(typeof(FriendshipRemovedMessage), "FriendshipRemoved")]
public abstract class NotificationMessage(NotificationType type)
{
    public NotificationType Type { get; } = type;
}