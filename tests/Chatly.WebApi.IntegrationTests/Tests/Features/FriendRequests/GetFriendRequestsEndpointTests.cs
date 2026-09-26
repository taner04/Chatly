using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.FriendRequests;

public sealed class GetFriendRequestsEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task GetFriendRequests_Should_ReturnOnlyIncomingPendingRequests_When_UserHasSentAndReceivedRequests()
    {
        var incoming = await CreateUserAsync("incoming");
        var outgoing = await CreateUserAsync("outgoing");
        await CreateAuthenticatedClient(incoming).SendFriendRequestAsync(
            new SendFriendRequestRequest(CurrentUser.Id.Value),
            CurrentCancellationToken);
        var client = CreateAuthenticatedClient();
        await client.SendFriendRequestAsync(new SendFriendRequestRequest(outgoing.Id.Value), CurrentCancellationToken);

        var response = await client.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(incoming.Id.Value, Assert.Single(response.Content!.Items).SenderUserId);
        Assert.Equal(1, response.Content.Pagination.TotalCount);
    }

    [Fact]
    public async Task GetFriendRequests_Should_PageResults_When_MoreRequestsThanPageSize()
    {
        foreach (var name in new[] { "sender_a", "sender_b", "sender_c" })
        {
            var sender = await CreateUserAsync(name);
            await CreateAuthenticatedClient(sender).SendFriendRequestAsync(
                new SendFriendRequestRequest(CurrentUser.Id.Value),
                CurrentCancellationToken);
        }

        var response = await CreateAuthenticatedClient().GetFriendRequestsAsync(1, 2, CurrentCancellationToken);

        Assert.Equal(2, response.Content!.Items.Count);
        Assert.Equal(3, response.Content.Pagination.TotalCount);
        Assert.Equal(2, response.Content.Pagination.NextPageIndex);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetFriendRequests_Should_Return400_When_PaginationIsInvalid(int pageIndex, int pageSize)
    {
        var response = await CreateAuthenticatedClient().GetFriendRequestsAsync(pageIndex, pageSize, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
