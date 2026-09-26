using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.FriendRequests;

public sealed class RejectFriendRequestEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task RejectFriendRequest_Should_Return204AndRemovePendingRequest_When_ReceiverRejects()
    {
        var sender = await CreateUserAsync("sender");
        await CreateAuthenticatedClient(sender).SendFriendRequestAsync(
            new SendFriendRequestRequest(CurrentUser.Id.Value),
            CurrentCancellationToken);
        var client = CreateAuthenticatedClient();
        var pending = await client.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        var requestId = Assert.Single(pending.Content!.Items).FriendRequestId;

        var response = await client.RejectFriendRequestAsync(requestId, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var remaining = await client.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        Assert.Empty(remaining.Content!.Items);
        await using var dbContext = GetDbContext();
        Assert.False(await dbContext.Friendships.AnyAsync(CurrentCancellationToken));
    }
}
