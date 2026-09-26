using Hangfire;
using Hangfire.States;

namespace Chatly.WebApi.Common.Infrastructure.Hangfire.RecurringJobs.Absence;

[ScopedService(typeof(IRecurringJob))]
internal sealed class AbsenceRecurringJob(
    IBackgroundJobClient backgroundJobClient,
    ChatlyDbContext context) : IRecurringJob
{
    public string Id => "absence-job";
    public string Queue => EnqueuedState.DefaultQueue;
    public string CronExpression => Cron.Daily();

    [DisableConcurrentExecution(3600)]
    public async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var thresholdDate = DateTimeOffset.UtcNow.AddDays(-7);

        var inactiveUserIds = context.Users
            .AsNoTracking()
            .Where(AbsenceDbQueryFilter.IsInactiveSince(thresholdDate))
            .Select(u => u.Id)
            .OrderBy(userId => userId)
            .AsAsyncEnumerable();

        await foreach (var userId in inactiveUserIds.WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            backgroundJobClient.Enqueue<SendAbsenceEmailJob>(job => job.ExecuteAsync(
                userId,
                CancellationToken.None));
        }
    }
}