using Chatly.Contracts.Features.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Features.Hubs;

[Authorize]
public abstract class HubBase<THubClient> : Hub<THubClient> where THubClient : class, IHubClient
{
    protected HubBase(ChatlyDbContext context)
    {
        Database = context;
    }

    protected ChatlyDbContext Database { get; }

    protected async Task<UserId> GetCurrentUserIdAsync(CancellationToken cancellationToken)
    {
        if (Context.Items.TryGetValue(CurrentUserService.UserIdItemKey, out var value) && value is UserId userId)
        {
            SetHttpContextUserId(userId);
            return userId;
        }

        var auth0Id = Context.User?.FindFirst(CurrentUserService.SubClaim)?.Value
                      ?? throw new HubException("The user is not authenticated.");
        userId = await Database.Users
            .Where(user => user.Auth0Id == auth0Id)
            .Select(user => user.Id)
            .SingleAsync(cancellationToken);
        Context.Items[CurrentUserService.UserIdItemKey] = userId;
        SetHttpContextUserId(userId);
        return userId;
    }

    private protected Task<List<UserId>> GetFriendUserIdsAsync(UserId userId, CancellationToken cancellationToken) =>
        Database.Friendships
            .AsNoTracking()
            .ForUser(userId)
            .SelectOtherUserId(userId)
            .ToListAsync(cancellationToken);

    public override async Task OnConnectedAsync()
    {
        var userId = await GetCurrentUserIdAsync(Context.ConnectionAborted);
        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.User(userId));
        await base.OnConnectedAsync();
    }

    protected UserId? GetConnectedUserId() =>
        Context.Items.TryGetValue(CurrentUserService.UserIdItemKey, out var value) && value is UserId userId
            ? userId
            : null;

    private void SetHttpContextUserId(UserId userId)
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is not null)
        {
            httpContext.Items[CurrentUserService.UserIdItemKey] = userId;
        }
    }
}