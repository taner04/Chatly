using System.Text.Json.Serialization;
using Chatly.Contracts.Features.FriendRequests.Endpoints.AcceptFriendRequest;
using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Contracts.Features.Friendships.Endpoints.RemoveFriendship;
using Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;
using Chatly.Contracts.Features.Messages.Endpoints.RemoveMessage;
using Chatly.Contracts.Features.Messages.Endpoints.SendMessage;
using Chatly.Contracts.Features.Reactions.Notifications;
using Chatly.Contracts.Features.Users.Notifications;

namespace Chatly.Contracts.Features.Hubs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$notificationType")]
[JsonDerivedType(typeof(IncomingFriendRequestNotification), "IncomingFriendRequest")]
[JsonDerivedType(typeof(FriendRequestAcceptedNotification), "FriendRequestAccepted")]
[JsonDerivedType(typeof(IncomingMessageNotification), "IncomingMessage")]
[JsonDerivedType(typeof(MessageDeletedNotification), "MessageDeleted")]
[JsonDerivedType(typeof(ReactionChangedNotification), "ReactionChanged")]
[JsonDerivedType(typeof(TypingStatusChangedNotification), "TypingStatusChanged")]
[JsonDerivedType(typeof(OnlineStatusChangedNotification), "OnlineStatusChanged")]
[JsonDerivedType(typeof(FriendshipRemovedNotification), "FriendshipRemoved")]
[JsonDerivedType(typeof(UserProfileUpdatedNotification), "UserProfileUpdated")]
public abstract class NotificationMessage : IHubMessage;