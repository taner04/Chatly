using Chatly.Contracts.Features.Hubs;
using Hangfire;
using Hangfire.States;

namespace Chatly.WebApi.Features.Calls.Jobs;

[ScopedService(typeof(IRecurringJob))]
internal sealed class CallExpiryRecurringJob(
    ChatlyDbContext context,
    CallExpiryJob expiryJob) : IRecurringJob
{
    public string Id => "call-expiry-job";
    public string Queue => EnqueuedState.DefaultQueue;
    public string CronExpression => Cron.Minutely();

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var ringingThreshold = now - CallExpiryJob.RingingTimeout;
        var maximumThreshold = now - CallExpiryJob.MaximumCallLifetime;
        var staleCallIds = await context.Calls
            .AsNoTracking()
            .Where(call => call.Status != CallState.Ended)
            .Where(call =>
                (call.Status == CallState.Ringing && call.InitiatedAt <= ringingThreshold) ||
                (call.Status != CallState.Ringing && call.InitiatedAt <= maximumThreshold))
            .Select(call => call.Id)
            .ToListAsync(cancellationToken);

        foreach (var callId in staleCallIds)
        {
            await expiryJob.ExecuteAsync(callId.Value, cancellationToken);
        }
    }
}