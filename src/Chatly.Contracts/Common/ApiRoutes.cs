namespace Chatly.Contracts.Common;

public static class ApiRoutes
{
    public static class Chats
    {
        public const string Collection = "/api/chats";
        public const string ChatMessages = "/api/chats/{chatId}/messages";
        public const string Read = "/api/chats/{chatId}/read";
    }

    public static class FriendRequests
    {
        public const string Collection = "/api/friend-requests";
        public const string Accept = "/api/friend-requests/{friendRequestId}/accept";
        public const string Reject = "/api/friend-requests/{friendRequestId}/reject";
    }

    public static class Friendships
    {
        public const string Collection = "/api/friendships";
        public const string ByAssociatedUserId = "/api/friendships/{associatedUserId}";
    }

    public static class Health
    {
        public const string Status = "/api/health/status";
    }

    public static class Hubs
    {
        public const string Notification = "/hubs/notification";
        public const string Call = "/hubs/call";
    }

    public static class Identity
    {
        public const string BackchannelLogout = "/api/identity/backchannel-logout";
    }

    public static class LiveKit
    {
        public const string Webhook = "/api/livekit/webhook";
    }

    public static class Messages
    {
        public const string Collection = "/api/messages";
        public const string ById = "/api/messages/{messageId}";
        public const string Reaction = "/api/messages/{messageId}/reaction";
    }

    public static class Reactions
    {
        public const string ById = "/api/reactions/{reactionId}";
    }

    public static class Users
    {
        public const string Current = "/api/users/me";
        public const string Onboarding = "/api/users/me/onboarding";
        public const string ProfilePicture = "/api/users/me/profile-picture";
        public const string Search = "/api/users/search";
        public const string Sessions = "/api/users/me/sessions";
        public const string CurrentSession = "/api/users/me/sessions/current";
        public const string SessionById = "/api/users/me/sessions/{sessionId}";
        public const string Username = "/api/users/me/username";
    }
}