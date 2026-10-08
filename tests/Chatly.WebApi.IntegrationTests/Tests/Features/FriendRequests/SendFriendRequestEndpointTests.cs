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

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendFriendRequest_Should_Return204AndShowRequestToReceiver_When_UsersAreNotFriends()
    {
        var receiver = await CreateUserAsync("receiver");

        var response = await CreateAuthenticatedClient().SendFriendRequestAsync(
            new SendFriendRequestRequest(receiver.Id.Value),
            CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var pending = await CreateAuthenticatedClient(receiver).GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        var request = pending.Content!.Items.Should().ContainSingle().Subject;
        request.SenderUserId.Should().Be(CurrentUser.Id.Value);
        request.SenderUsername.Should().Be(CurrentUser.Username);
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

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
        var requestId = pending.Content!.Items.Should().ContainSingle().Subject.FriendRequestId;
        await receiverClient.RejectFriendRequestAsync(requestId, CurrentCancellationToken);

        var response = await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value),
            CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var again = await receiverClient.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        again.Content!.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task SendFriendRequest_Should_BePossibleAgain_When_FriendshipWasRemoved()
    {
        var receiver = await CreateUserAsync("receiver");
        var client = CreateAuthenticatedClient();
        var receiverClient = CreateAuthenticatedClient(receiver);
        await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value), CurrentCancellationToken);
        var pending = await receiverClient.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        var requestId = pending.Content!.Items.Should().ContainSingle().Subject.FriendRequestId;
        await receiverClient.AcceptFriendRequestAsync(requestId, CurrentCancellationToken);
        await client.RemoveFriendshipAsync(receiver.Id.Value, CurrentCancellationToken);

        var response = await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value),
            CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SendFriendRequest_Should_Return409_When_UsersAreAlreadyFriends()
    {
        var receiver = await CreateUserAsync("receiver");
        var client = CreateAuthenticatedClient();
        var receiverClient = CreateAuthenticatedClient(receiver);
        await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value), CurrentCancellationToken);
        var pending = await receiverClient.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        var requestId = pending.Content!.Items.Should().ContainSingle().Subject.FriendRequestId;
        await receiverClient.AcceptFriendRequestAsync(requestId, CurrentCancellationToken);

        var response = await client.SendFriendRequestAsync(new SendFriendRequestRequest(receiver.Id.Value),
            CurrentCancellationToken);

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
        var requestId = pending.Content!.Items.Should().ContainSingle().Subject.FriendRequestId;
        await otherClient.RejectFriendRequestAsync(requestId, CurrentCancellationToken);

        var response = await otherClient.SendFriendRequestAsync(
            new SendFriendRequestRequest(CurrentUser.Id.Value),
            CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var incoming = await client.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        incoming.Content!.Items.Should().ContainSingle().Subject.SenderUserId.Should().Be(other.Id.Value);
        var outgoing = await otherClient.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);
        outgoing.Content!.Items.Should().BeEmpty();
    }
}