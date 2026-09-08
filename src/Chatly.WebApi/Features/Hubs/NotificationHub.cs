using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Features.Hubs;

internal static class NotificationHubGroups
{
    internal static string User(UserId userId)
    {
        return $"user:{userId.Value}";
    }
}

[Authorize]
public sealed partial class NotificationHub(
    CurrentUserService currentUser,
    ChatlyDbContext context,
    OnlinePresenceTracker presenceTracker) : Hub<INotificationHubClient>, INotificationHubServer
{
    private const string UserIdContextKey = "Chatly.UserId";

    private async Task<UserId> ValidateUserIdAsync(Guid userId)
    {
        var authenticatedUserId = await GetCurrentUserIdAsync(Context.ConnectionAborted);
        if (authenticatedUserId.Value != userId)
        {
            throw new HubException("The user ID does not match the authenticated user.");
        }

        return authenticatedUserId;
    }

    private async Task<UserId> GetCurrentUserIdAsync(CancellationToken cancellationToken)
    {
        if (Context.Items.TryGetValue(UserIdContextKey, out var value) && value is UserId userId)
        {
            return userId;
        }

        var auth0Id = currentUser.GetAuth0Id();
        userId = await context.Users
            .Where(user => user.Auth0Id == auth0Id)
            .Select(user => user.Id)
            .SingleAsync(cancellationToken);
        Context.Items[UserIdContextKey] = userId;
        return userId;
    }

    private Task<List<UserId>> GetFriendUserIdsAsync(UserId userId, CancellationToken cancellationToken)
    {
        return context.Friendships
            .AsNoTracking()
            .Where(friendship => friendship.FirstUserId == userId || friendship.SecondUserId == userId)
            .Select(friendship => friendship.FirstUserId == userId
                ? friendship.SecondUserId
                : friendship.FirstUserId)
            .ToListAsync(cancellationToken);
    }
}
