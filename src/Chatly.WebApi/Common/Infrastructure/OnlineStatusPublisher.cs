using Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;
using Chatly.WebApi.Features.Hubs;
using Chatly.WebApi.Features.Hubs.NotificationHub;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Common.Infrastructure;

[SingletonService]
public sealed partial class OnlineStatusPublisher(
    IServiceScopeFactory scopeFactory,
    IHubContext<NotificationHub, INotificationHubClient> hubContext,
    OnlinePresenceTracker presenceTracker,
    ILogger<OnlineStatusPublisher> logger)
{
    internal static readonly TimeSpan OfflineGracePeriod = TimeSpan.FromSeconds(5);

    internal async Task PublishAsync(UserId userId, bool isOnline, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ChatlyDbContext>();
        var friendUserIds = await context.Friendships
            .AsNoTracking()
            .ForUser(userId)
            .SelectOtherUserId(userId)
            .ToListAsync(cancellationToken);

        var notification = new OnlineStatusChangedNotification(userId.Value, isOnline);
        await Task.WhenAll(friendUserIds.Select(friendUserId =>
            hubContext.Clients.Group(HubGroups.User(friendUserId)).Receive(notification)));
    }

    internal void PublishOfflineAfterGracePeriod(UserId userId, long offlineVersion) =>
        _ = PublishOfflineAfterGracePeriodAsync(userId, offlineVersion);

    private async Task PublishOfflineAfterGracePeriodAsync(UserId userId, long offlineVersion)
    {
        try
        {
            await Task.Delay(OfflineGracePeriod);
            if (presenceTracker.TryCompleteOffline(userId, offlineVersion))
            {
                await PublishAsync(userId, false, CancellationToken.None);
            }
        }
        catch (Exception exception)
        {
            LogOfflinePublishFailed(userId.Value, exception);
        }
    }

    [LoggerMessage(LogLevel.Warning, "Failed to publish offline status for user {UserId}.")]
    private partial void LogOfflinePublishFailed(Guid userId, Exception exception);
}