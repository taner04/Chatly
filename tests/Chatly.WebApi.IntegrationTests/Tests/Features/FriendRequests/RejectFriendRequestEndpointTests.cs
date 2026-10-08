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
        var requestId = pending.Content!.Items.Should().ContainSingle().Subject.FriendRequestId;

        var response = await client.RejectFriendRequestAsync(requestId, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var remaining = await client.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        remaining.Content!.Items.Should().BeEmpty();
        await using var dbContext = GetDbContext();
        (await dbContext.Friendships.AnyAsync(CurrentCancellationToken)).Should().BeFalse();
    }
}