using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.FriendRequests;

public sealed class AcceptFriendRequestEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task AcceptFriendRequest_Should_CreateFriendshipAndChat_When_ReceiverAccepts()
    {
        var sender = await CreateUserAsync("sender");
        await CreateAuthenticatedClient(sender).SendFriendRequestAsync(
            new SendFriendRequestRequest(CurrentUser.Id.Value),
            CurrentCancellationToken);
        var client = CreateAuthenticatedClient();
        var pending = await client.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        var requestId = Assert.Single(pending.Content!.Items).FriendRequestId;

        var response = await client.AcceptFriendRequestAsync(requestId, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(sender.Id.Value, response.Content!.FriendUserId);
        var friendships = await client.GetFriendshipsAsync(CurrentCancellationToken);
        Assert.Equal(sender.Id.Value, Assert.Single(friendships.Content!).FriendUserId);
        var chats = await CreateAuthenticatedClient(sender).GetChatsAsync(CurrentCancellationToken);
        Assert.Equal(response.Content.DirectChatId, Assert.Single(chats.Content!).ChatId);
    }

    [Fact]
    public async Task AcceptFriendRequest_Should_Return404_When_SenderTriesToAcceptOwnRequest()
    {
        var receiver = await CreateUserAsync("receiver");
        var client = CreateAuthenticatedClient();
        await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value), CurrentCancellationToken);
        var pending = await CreateAuthenticatedClient(receiver).GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        var requestId = Assert.Single(pending.Content!.Items).FriendRequestId;

        var response = await client.AcceptFriendRequestAsync(requestId, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await using var dbContext = GetDbContext();
        Assert.False(await dbContext.Friendships.AnyAsync(CurrentCancellationToken));
    }
}