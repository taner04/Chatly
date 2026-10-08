using Hangfire;
using Hangfire.States;

namespace Chatly.WebApi.Features.DeviceSessions.Jobs;

[ScopedService(typeof(IRecurringJob))]
internal sealed class DeviceSessionCleanupRecurringJob(ChatlyDbContext context) : IRecurringJob
{
    internal static readonly TimeSpan RevokedSessionRetention = TimeSpan.FromDays(30);
    internal static readonly TimeSpan InactiveSessionRetention = TimeSpan.FromDays(30);

    public string Id => "device-session-cleanup-job";
    public string Queue => EnqueuedState.DefaultQueue;
    public string CronExpression => Cron.Daily();

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var revokedThreshold = now - RevokedSessionRetention;
        var inactiveThreshold = now - InactiveSessionRetention;

        await context.DeviceSessions
            .Where(session => (session.RevokedAt != null && session.RevokedAt <= revokedThreshold)
                              || session.LastSeenAt <= inactiveThreshold)
            .ExecuteDeleteAsync(cancellationToken);
    }
}