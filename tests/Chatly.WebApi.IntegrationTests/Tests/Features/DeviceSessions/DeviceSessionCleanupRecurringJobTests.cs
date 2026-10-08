using Chatly.WebApi.Common.Abstraction;
using Chatly.WebApi.Features.DeviceSessions.Jobs;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.DeviceSessions;

public sealed class DeviceSessionCleanupRecurringJobTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task Execute_Should_DeleteOnlyLongRevokedSessions_When_RetentionPassed()
    {
        var expiredAt = DateTimeOffset.UtcNow
                        - DeviceSessionCleanupRecurringJob.RevokedSessionRetention
                        - TimeSpan.FromDays(1);
        var expiredSessionId = await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "old", expiredAt);
        var recentlyRevokedSessionId = await CreateDeviceSessionAsync(
            CurrentUser,
            Guid.NewGuid(),
            "recent",
            DateTimeOffset.UtcNow.AddDays(-1));
        var activeSessionId = await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "active");

        await RunJobAsync();

        await using var dbContext = GetDbContext();
        var remainingSessionIds = await dbContext.DeviceSessions
            .Select(session => session.Id)
            .ToListAsync(CurrentCancellationToken);
        remainingSessionIds.Should().BeEquivalentTo([recentlyRevokedSessionId, activeSessionId]);
        remainingSessionIds.Should().NotContain(expiredSessionId);
    }

    [Fact]
    public async Task Execute_Should_DeleteActiveSession_When_DeviceWasNotSeenForRetention()
    {
        var staleSessionId = await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "stale");
        var recentSessionId = await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "recent");
        var staleSince = DateTimeOffset.UtcNow
                         - DeviceSessionCleanupRecurringJob.InactiveSessionRetention
                         - TimeSpan.FromDays(1);
        await using (var setup = GetDbContext())
        {
            await setup.DeviceSessions
                .Where(session => session.Id == staleSessionId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(session => session.LastSeenAt, staleSince),
                    CurrentCancellationToken);
        }

        await RunJobAsync();

        await using var dbContext = GetDbContext();
        var remainingSessionIds = await dbContext.DeviceSessions
            .Select(session => session.Id)
            .ToListAsync(CurrentCancellationToken);
        remainingSessionIds.Should().Equal(recentSessionId);
    }

    private async Task RunJobAsync()
    {
        using var scope = CreateScope();
        var job = scope.ServiceProvider.GetServices<IRecurringJob>()
            .OfType<DeviceSessionCleanupRecurringJob>()
            .Single();
        await job.ExecuteAsync(CurrentCancellationToken);
    }
}