using System.Collections.Frozen;
using System.Diagnostics;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Chatly.WebApi.Features.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Chatly.MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime applicationLifetime,
    IHostEnvironment hostEnvironment)
    : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private readonly ActivitySource _sActivitySource = new(ActivitySourceName);


    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var activity = _sActivitySource.StartActivity(ActivityKind.Client);

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
        catch (Exception e)
        {
            activity?.AddException(e);
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
        await strategy.ExecuteAsync(async () => { await dbContext.Database.MigrateAsync(cancellationToken); });
    }

    private static async Task RunSeedAsync(
        ChatlyDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            var seedUsers = Enumerable.Range(1, 50)
                .Select(index => new
                {
                    Email = $"testuser{index:D3}@chatly.test",
                    Auth0Id = $"seed|user-{index:D3}",
                    Username = $"testuser{index:D3}"
                })
                .ToArray();

            var usersToAdd = seedUsers
                .Select(user => new User(user.Email, user.Auth0Id)
                {
                    Username = user.Username,
                    OnboardingCompleted = true,
                })
                .ToList();

            usersToAdd.ForEach(u => u.SetCreated("Seed"));

            dbContext.Users.AddRange(usersToAdd);
            await dbContext.SaveChangesAsync(cancellationToken);
        });
    }
}