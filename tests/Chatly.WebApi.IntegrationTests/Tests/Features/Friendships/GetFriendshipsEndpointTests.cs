namespace Chatly.WebApi.IntegrationTests.Tests.Features.Friendships;

public sealed class GetFriendshipsEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task GetFriendships_Should_ReturnFriendsWithTheirChat_When_UserHasFriends()
    {
        var friend = await CreateUserAsync("friend");
        await CreateUserAsync("stranger");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);

        var response = await CreateAuthenticatedClient().GetFriendshipsAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var friendship = response.Content!.Should().ContainSingle().Subject;
        friendship.FriendUserId.Should().Be(friend.Id.Value);
        friendship.FriendUsername.Should().Be(friend.Username);
        friendship.DirectChatId.Should().Be(chatId.Value);
        friendship.IsOnline.Should().BeFalse();
    }

    [Fact]
    public async Task GetFriendships_Should_ShowFriendOnline_When_FriendIsConnected()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var friendConnection = await ConnectNotificationHubAsync(friend);

        var client = CreateAuthenticatedClient();

        var onlineFriend = await WaitForAsync(async () =>
        {
            var response = await client.GetFriendshipsAsync(CurrentCancellationToken);
            return response.Content?.SingleOrDefault(friendship => friendship.IsOnline);
        });

        onlineFriend.Should().NotBeNull();
    }
}