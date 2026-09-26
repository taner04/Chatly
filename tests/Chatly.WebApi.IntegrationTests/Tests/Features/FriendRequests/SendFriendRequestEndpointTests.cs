using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.FriendRequests;

public sealed class SendFriendRequestEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task SendFriendRequest_Should_Return401_When_Unauthenticated()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.SendFriendRequestAsync(
            new SendFriendRequestRequest(Guid.NewGuid()),
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SendFriendRequest_Should_Return204AndShowRequestToReceiver_When_UsersAreNotFriends()
    {
        var receiver = await CreateUserAsync("receiver");

        var response = await CreateAuthenticatedClient().SendFriendRequestAsync(
            new SendFriendRequestRequest(receiver.Id.Value),
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var pending = await CreateAuthenticatedClient(receiver).GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        var request = Assert.Single(pending.Content!.Items);
        Assert.Equal(CurrentUser.Id.Value, request.SenderUserId);
        Assert.Equal(CurrentUser.Username, request.SenderUsername);
    }

    [Fact]
    public async Task SendFriendRequest_Should_Return409_When_RequestWasAlreadySent()
    {
        var receiver = await CreateUserAsync("receiver");
        var client = CreateAuthenticatedClient();
        await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value), CurrentCancellationToken);

        var response = await client.SendFriendRequestAsync(
            new SendFriendRequestRequest(receiver.Id.Value),
            CurrentCancellationToken);

        AssertError(response, HttpStatusCode.Conflict, "FriendRequest.AlreadySent");
    }

    [Fact]
    public async Task SendFriendRequest_Should_Return400_When_UserSendsRequestToThemselves()
    {
        var response = await CreateAuthenticatedClient().SendFriendRequestAsync(
            new SendFriendRequestRequest(CurrentUser.Id.Value),
            CurrentCancellationToken);

        AssertError(response, HttpStatusCode.BadRequest, "FriendRequest.ToSelf");
    }

    [Fact]
    public async Task SendFriendRequest_Should_Return404_When_ReceiverDoesNotExist()
    {
        var response = await CreateAuthenticatedClient().SendFriendRequestAsync(
            new SendFriendRequestRequest(Guid.NewGuid()),
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SendFriendRequest_Should_Return409_When_ReceiverAlreadySentARequest()
    {
        var other = await CreateUserAsync("other");
        await CreateAuthenticatedClient(other).SendFriendRequestAsync(
            new SendFriendRequestRequest(CurrentUser.Id.Value),
            CurrentCancellationToken);

        var response = await CreateAuthenticatedClient().SendFriendRequestAsync(
            new SendFriendRequestRequest(other.Id.Value),
            CurrentCancellationToken);

        AssertError(response, HttpStatusCode.Conflict, "FriendRequest.IncomingAlreadyExists");
    }

    [Fact]
    public async Task SendFriendRequest_Should_BePossibleAgain_When_PreviousRequestWasRejected()
    {
        var receiver = await CreateUserAsync("receiver");
        var client = CreateAuthenticatedClient();
        var receiverClient = CreateAuthenticatedClient(receiver);
        await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value), CurrentCancellationToken);
        var pending = await receiverClient.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        await receiverClient.RejectFriendRequestAsync(Assert.Single(pending.Content!.Items).FriendRequestId, CurrentCancellationToken);

        var response = await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value), CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var again = await receiverClient.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        Assert.Single(again.Content!.Items);
    }

    [Fact]
    public async Task SendFriendRequest_Should_BePossibleAgain_When_FriendshipWasRemoved()
    {
        var receiver = await CreateUserAsync("receiver");
        var client = CreateAuthenticatedClient();
        var receiverClient = CreateAuthenticatedClient(receiver);
        await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value), CurrentCancellationToken);
        var pending = await receiverClient.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        await receiverClient.AcceptFriendRequestAsync(Assert.Single(pending.Content!.Items).FriendRequestId, CurrentCancellationToken);
        await client.RemoveFriendshipAsync(receiver.Id.Value, CurrentCancellationToken);

        var response = await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value), CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SendFriendRequest_Should_Return409_When_UsersAreAlreadyFriends()
    {
        var receiver = await CreateUserAsync("receiver");
        var client = CreateAuthenticatedClient();
        var receiverClient = CreateAuthenticatedClient(receiver);
        await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value), CurrentCancellationToken);
        var pending = await receiverClient.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        await receiverClient.AcceptFriendRequestAsync(Assert.Single(pending.Content!.Items).FriendRequestId, CurrentCancellationToken);

        var response = await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value), CurrentCancellationToken);

        AssertError(response, HttpStatusCode.Conflict, "FriendRequest.AlreadyAccepted");
    }

    [Fact]
    public async Task SendFriendRequest_Should_ReverseDirection_When_RejectedReceiverSendsBack()
    {
        var other = await CreateUserAsync("other");
        var client = CreateAuthenticatedClient();
        var otherClient = CreateAuthenticatedClient(other);
        await client.SendFriendRequestAsync(new SendFriendRequestRequest(other.Id.Value), CurrentCancellationToken);
        var pending = await otherClient.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        await otherClient.RejectFriendRequestAsync(Assert.Single(pending.Content!.Items).FriendRequestId, CurrentCancellationToken);

        var response = await otherClient.SendFriendRequestAsync(
            new SendFriendRequestRequest(CurrentUser.Id.Value),
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var incoming = await client.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        Assert.Equal(other.Id.Value, Assert.Single(incoming.Content!.Items).SenderUserId);
        var outgoing = await otherClient.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        Assert.Empty(outgoing.Content!.Items);
    }
}
