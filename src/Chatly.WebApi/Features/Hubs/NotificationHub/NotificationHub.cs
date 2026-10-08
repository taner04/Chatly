using Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;
using Chatly.WebApi.Features.Chats.Services;
using Chatly.WebApi.Features.DeviceSessions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Features.Hubs.NotificationHub;

[Authorize]
public sealed class NotificationHub(
    ChatlyDbContext context,
    DeviceSessionService deviceSessionService,
    ChatAccessService chatAccessService,
    OnlinePresenceTracker presenceTracker,
    OnlineStatusPublisher onlineStatusPublisher) : HubBase<INotificationHubClient>(context, deviceSessionService),
    INotificationHubServer
{
    public Task StartTyping(Guid chatId) => PublishTypingStatusAsync(chatId, true);

    public Task StopTyping(Guid chatId) => PublishTypingStatusAsync(chatId, false);

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        var userId = CurrentUserId;
        await UpdateLastSeenAsync(userId, Context.ConnectionAborted);
        var cameOnline = presenceTracker.Connect(userId, Context.ConnectionId);
        try
        {
            if (cameOnline)
            {
                await onlineStatusPublisher.PublishAsync(userId, true, Context.ConnectionAborted);
            }

            await PublishOnlineFriendsSnapshotAsync(userId);
        }
        catch
        {
            if (presenceTracker.TryBeginOffline(userId, Context.ConnectionId, out var offlineVersion))
            {
                onlineStatusPublisher.PublishOfflineAfterGracePeriod(userId, offlineVersion);
            }

            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (GetConnectedUserId() is { } userId)
        {
            await UpdateLastSeenAsync(userId, CancellationToken.None);
            if (presenceTracker.TryBeginOffline(userId, Context.ConnectionId, out var offlineVersion))
            {
                onlineStatusPublisher.PublishOfflineAfterGracePeriod(userId, offlineVersion);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task PublishTypingStatusAsync(Guid chatId, bool isTyping)
    {
        var userId = CurrentUserId;
        var chat = await chatAccessService.GetAsync(
                       ChatId.From(chatId),
                       userId,
                       Context.ConnectionAborted)
                   ?? throw new HubException("Chat was not found.");

        await Clients.Group(HubGroups.User(chat.OtherParticipantUserId))
            .Receive(new TypingStatusChangedNotification(chatId, isTyping));
    }

    private async Task PublishOnlineFriendsSnapshotAsync(UserId userId)
    {
        var friendUserIds = await GetFriendUserIdsAsync(userId, Context.ConnectionAborted);
        foreach (var friendUserId in friendUserIds.Where(presenceTracker.IsOnline))
        {
            await Clients.Caller.Receive(new OnlineStatusChangedNotification(friendUserId.Value, true));
        }
    }

    private Task UpdateLastSeenAsync(UserId userId, CancellationToken cancellationToken) =>
        Database.Users
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    user => user.LastSeenAt,
                    DateTimeOffset.UtcNow),
                cancellationToken);
}