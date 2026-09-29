using Chatly.Contracts.Features.Hubs;
using Chatly.WebApi.Features.Calls.Jobs;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Calls;

public sealed class CallExpiryJobTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task Execute_Should_EndRingingCallAsMissed_When_RingingTimeoutPassed()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        var started = await caller.StartCallAsync(friend.Id.Value);
        await using (var dbContext = GetDbContext())
        {
            await dbContext.Database.ExecuteSqlAsync(
                $"UPDATE \"Calls\" SET \"InitiatedAt\" = now() - interval '5 minutes'",
                CurrentCancellationToken);
        }

        using (var scope = CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<CallExpiryJob>()
                .ExecuteAsync(started.CallId, CurrentCancellationToken);
        }

        await using var assertContext = GetDbContext();
        var call = await assertContext.Calls.SingleAsync(CurrentCancellationToken);
        Assert.Equal(CallState.Ended, call.Status);
        Assert.Equal(CallEndReason.Missed, call.EndReason);
        Assert.False(await assertContext.ActiveCallParticipants.AnyAsync(CurrentCancellationToken));
    }

    [Fact]
    public async Task Execute_Should_KeepRingingCall_When_RingingTimeoutHasNotPassed()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        var started = await caller.StartCallAsync(friend.Id.Value);

        using (var scope = CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<CallExpiryJob>()
                .ExecuteAsync(started.CallId, CurrentCancellationToken);
        }

        await using var dbContext = GetDbContext();
        Assert.Equal(CallState.Ringing, (await dbContext.Calls.SingleAsync(CurrentCancellationToken)).Status);
    }

    [Fact]
    public async Task Execute_Should_EndActiveCallAsFailed_When_MaximumLifetimePassed()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiver = await ConnectCallHubAsync(friend);
        var started = await caller.StartCallAsync(friend.Id.Value);
        await receiver.AcceptCallAsync(started.CallId);
        await using (var dbContext = GetDbContext())
        {
            await dbContext.Database.ExecuteSqlAsync(
                $"UPDATE \"Calls\" SET \"InitiatedAt\" = now() - interval '13 hours'",
                CurrentCancellationToken);
        }

        using (var scope = CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<CallExpiryJob>()
                .ExecuteAsync(started.CallId, CurrentCancellationToken);
        }

        await using var assertContext = GetDbContext();
        var call = await assertContext.Calls.SingleAsync(CurrentCancellationToken);
        Assert.Equal(CallState.Ended, call.Status);
        Assert.Equal(CallEndReason.Failed, call.EndReason);
        Assert.False(await assertContext.ActiveCallParticipants.AnyAsync(CurrentCancellationToken));
    }
}