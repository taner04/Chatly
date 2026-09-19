namespace Chatly.WebApi.Features.Hubs;

internal static class NotificationHubGroups
{
    internal static string User(UserId userId) => $"user:{userId.Value}";
}