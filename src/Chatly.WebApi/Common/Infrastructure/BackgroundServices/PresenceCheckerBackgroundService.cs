namespace Chatly.WebApi.Common.Infrastructure.BackgroundServices;

internal sealed partial class PresenceCheckerBackgroundService(
    IServiceScopeFactory scopeFactory,
    OnlinePresenceTracker presenceTracker,
    ILogger<PresenceCheckerBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await UpdateLastSeenAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                LogHeartbeatFailed(exception);
            }
        }
    }

    private async Task UpdateLastSeenAsync(CancellationToken cancellationToken)
    {
        var userIds = presenceTracker.GetOnlineUserIds();
        if (userIds.Count == 0)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ChatlyDbContext>();

        await context.Users
            .Where(user => userIds.Contains(user.Id))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    user => user.LastSeenAt,
                    DateTimeOffset.UtcNow),
                cancellationToken);
    }

    [LoggerMessage(LogLevel.Warning, "Failed to update online-user presence timestamps.")]
    private partial void LogHeartbeatFailed(Exception exception);
}