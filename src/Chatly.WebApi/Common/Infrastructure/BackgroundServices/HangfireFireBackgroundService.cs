using Hangfire;
using Hangfire.Server;

namespace Chatly.WebApi.Common.Infrastructure.BackgroundServices;

internal sealed class HangfireFireBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        foreach (var filter in services.GetServices<IServerFilter>())
        {
            GlobalJobFilters.Filters.Add(filter);
        }

        var jobManager = services.GetRequiredService<IRecurringJobManager>();

        foreach (var job in services.GetServices<IRecurringJob>())
        {
            jobManager.AddOrUpdate(
                job.Id,
                job.Queue,
                () => job.ExecuteAsync(CancellationToken.None),
                job.CronExpression);
        }

        return Task.CompletedTask;
    }
}
