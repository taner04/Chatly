using Chatly.Contracts.SignalR;

namespace Chatly.WebApi.Features.Hubs;

public sealed partial class NotificationHub
{
    public async Task IsOnline(Guid userId)
    {
        var authenticatedUserId = await ValidateUserIdAsync(userId);
        if (presenceTracker.Connect(authenticatedUserId, Context.ConnectionId))
        {
            await PublishOnlineStatusAsync(authenticatedUserId, true, Context.ConnectionAborted);
        }
    }

    public async Task IsOffline(Guid userId)
    {
        var authenticatedUserId = await ValidateUserIdAsync(userId);
        if (presenceTracker.Disconnect(authenticatedUserId, Context.ConnectionId))
        {
            await PublishOnlineStatusAsync(authenticatedUserId, false, Context.ConnectionAborted);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = await GetCurrentUserIdAsync(Context.ConnectionAborted);
        await Groups.AddToGroupAsync(Context.ConnectionId, NotificationHubGroups.User(userId));

        if (presenceTracker.Connect(userId, Context.ConnectionId))
        {
            await PublishOnlineStatusAsync(userId, true, Context.ConnectionAborted);
        }

        await PublishOnlineFriendsSnapshotAsync(userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(UserIdContextKey, out var value) && value is UserId userId &&
            presenceTracker.Disconnect(userId, Context.ConnectionId))
        {
            await PublishOnlineStatusAsync(userId, false, CancellationToken.None);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task PublishOnlineStatusAsync(
        UserId userId,
        bool isOnline,
        CancellationToken cancellationToken)
    {
        var friendUserIds = await GetFriendUserIdsAsync(userId, cancellationToken);
        var message = new OnlineStatusChangedMessage(userId.Value, isOnline);

        await Task.WhenAll(friendUserIds.Select(friendUserId =>
            Clients.Group(NotificationHubGroups.User(friendUserId)).Receive(message)));
    }

    private async Task PublishOnlineFriendsSnapshotAsync(UserId userId)
    {
        var friendUserIds = await GetFriendUserIdsAsync(userId, Context.ConnectionAborted);

        foreach (var friendUserId in friendUserIds.Where(presenceTracker.IsOnline))
        {
            await Clients.Caller.Receive(new OnlineStatusChangedMessage(friendUserId.Value, true));
        }
    }
}