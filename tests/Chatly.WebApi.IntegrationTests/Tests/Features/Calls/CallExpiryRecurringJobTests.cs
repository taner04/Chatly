using Chatly.WebApi.Common.Abstraction;
using Chatly.WebApi.Features.Calls.Enums;
using Chatly.WebApi.Features.Calls.Jobs;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Calls;

public sealed class CallExpiryRecurringJobTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task Execute_Should_ExpireOnlyStaleCalls_When_SomeCallsTimedOut()
    {
        var staleFriend = await CreateUserAsync("stale_friend");
        var freshCaller = await CreateUserAsync("fresh_caller");
        var freshFriend = await CreateUserAsync("fresh_friend");
        await CreateFriendshipAsync(CurrentUser, staleFriend);
        await CreateFriendshipAsync(freshCaller, freshFriend);
        await using var staleHub = await ConnectCallHubAsync();
        await using var freshHub = await ConnectCallHubAsync(freshCaller);
        var stale = await staleHub.StartCallAsync(staleFriend.Id.Value);
        var fresh = await freshHub.StartCallAsync(freshFriend.Id.Value);
        await using (var dbContext = GetDbContext())
        {
            await dbContext.Database.ExecuteSqlAsync(
                $"UPDATE \"Calls\" SET \"InitiatedAt\" = now() - interval '5 minutes' WHERE \"Id\" = {stale.CallId}",
                CurrentCancellationToken);
        }

        using (var scope = CreateScope())
        {
            var job = scope.ServiceProvider.GetServices<IRecurringJob>().OfType<CallExpiryRecurringJob>().Single();
            await job.ExecuteAsync(CurrentCancellationToken);
        }

        await using var assertContext = GetDbContext();
        var calls = await assertContext.Calls.ToDictionaryAsync(call => call.Id.Value, CurrentCancellationToken);
        Assert.Equal(CallStatus.Ended, calls[stale.CallId].Status);
        Assert.Equal(CallEndReason.Missed, calls[stale.CallId].EndReason);
        Assert.Equal(CallStatus.Ringing, calls[fresh.CallId].Status);
    }
}
