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
        var requestId = pending.Content!.Items.Should().ContainSingle().Subject.FriendRequestId;

        var response = await client.AcceptFriendRequestAsync(requestId, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.FriendUserId.Should().Be(sender.Id.Value);
        var friendships = await client.GetFriendshipsAsync(CurrentCancellationToken);
        friendships.Content!.Should().ContainSingle().Subject.FriendUserId.Should().Be(sender.Id.Value);
        var chats = await CreateAuthenticatedClient(sender).GetChatsAsync(CurrentCancellationToken);
        chats.Content!.Should().ContainSingle().Subject.ChatId.Should().Be(response.Content.DirectChatId);
    }

    [Fact]
    public async Task AcceptFriendRequest_Should_Return404_When_SenderTriesToAcceptOwnRequest()
    {
        var receiver = await CreateUserAsync("receiver");
        var client = CreateAuthenticatedClient();
        await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value), CurrentCancellationToken);
        var pending = await CreateAuthenticatedClient(receiver).GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        var requestId = pending.Content!.Items.Should().ContainSingle().Subject.FriendRequestId;

        var response = await client.AcceptFriendRequestAsync(requestId, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await using var dbContext = GetDbContext();
        (await dbContext.Friendships.AnyAsync(CurrentCancellationToken)).Should().BeFalse();
    }
}