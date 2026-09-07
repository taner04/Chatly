using System.Text.Json.Serialization;

namespace Chatly.Contracts.SignalR;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    Unknown,
    IncomingFriendRequest,
    FriendRequestAccepted,
    IncomingMessage,
    TypingStatusChanged,
    OnlineStatusChanged,
    FriendshipRemoved,
    UserProfileUpdated
}