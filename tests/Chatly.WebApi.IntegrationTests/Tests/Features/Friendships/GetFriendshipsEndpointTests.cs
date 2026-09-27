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

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var friendship = Assert.Single(response.Content!);
        Assert.Equal(friend.Id.Value, friendship.FriendUserId);
        Assert.Equal(friend.Username, friendship.FriendUsername);
        Assert.Equal(chatId.Value, friendship.DirectChatId);
        Assert.False(friendship.IsOnline);
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

        Assert.NotNull(onlineFriend);
    }
}
