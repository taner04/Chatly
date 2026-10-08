using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Friendships;

public sealed class RemoveFriendshipEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task RemoveFriendship_Should_Return204AndHideChat_When_UsersAreFriends()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();

        var response = await client.RemoveFriendshipAsync(friend.Id.Value, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var friendships = await client.GetFriendshipsAsync(CurrentCancellationToken);
        friendships.Content!.Should().BeEmpty();
        var send = await client.SendMessageAsync(chatId.Value, "still there?", CurrentCancellationToken);
        send.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await using var dbContext = GetDbContext();
        (await dbContext.Chats.AnyAsync(chat => chat.Id == chatId, CurrentCancellationToken)).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveFriendship_Should_EndRunningCall_When_UsersAreInACall()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiver = await ConnectCallHubAsync(friend);
        var started = await caller.StartCallAsync(friend.Id.Value);

        var response = await CreateAuthenticatedClient()
            .RemoveFriendshipAsync(friend.Id.Value, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var ended = await receiver.ReceiveAsync<CallEndedNotification>();
        ended.CallId.Should().Be(started.CallId);
        ended.Reason.Should().Be(CallEndReason.Cancelled);
        await using var dbContext = GetDbContext();
        (await dbContext.Calls.SingleAsync(CurrentCancellationToken)).Status.Should().Be(CallState.Ended);
        (await dbContext.ActiveCallParticipants.AnyAsync(CurrentCancellationToken)).Should().BeFalse();
    }
}