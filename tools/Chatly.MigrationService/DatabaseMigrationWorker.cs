using System.Diagnostics;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Chatly.MigrationService;

public sealed partial class DatabaseMigrationWorker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime applicationLifetime,
    IHostEnvironment hostEnvironment)
    : BackgroundService
{
    internal const string ActivitySourceName = "Migrations";
    private readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = _activitySource.StartActivity(ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ChatlyDbContext>();

            await RunMigrationAsync(dbContext, stoppingToken);

            if (hostEnvironment.IsDevelopment())
            {
                await RunSeedAsync(dbContext, stoppingToken);
            }
        }
        catch (Exception exception)
        {
            activity?.AddException(exception);
            throw;
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    private static async Task RunMigrationAsync(
        ChatlyDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => await dbContext.Database.MigrateAsync(cancellationToken));
    }
}
