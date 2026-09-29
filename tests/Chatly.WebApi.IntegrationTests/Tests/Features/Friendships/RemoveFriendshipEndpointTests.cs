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

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var friendships = await client.GetFriendshipsAsync(CurrentCancellationToken);
        Assert.Empty(friendships.Content!);
        var send = await client.SendMessageAsync(chatId.Value, "still there?", CurrentCancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, send.StatusCode);
        await using var dbContext = GetDbContext();
        Assert.True(await dbContext.Chats.AnyAsync(chat => chat.Id == chatId, CurrentCancellationToken));
    }
}