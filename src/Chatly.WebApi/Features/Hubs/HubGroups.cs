namespace Chatly.WebApi.Features.Hubs;

internal static class HubGroups
{
    internal static string User(UserId userId) => $"user:{userId.Value}";
}