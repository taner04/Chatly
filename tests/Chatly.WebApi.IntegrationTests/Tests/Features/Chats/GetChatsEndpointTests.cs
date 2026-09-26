namespace Chatly.WebApi.IntegrationTests.Tests.Features.Chats;

public sealed class GetChatsEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task GetChats_Should_ReturnFriendChatWithUnreadCount_When_FriendSentMessages()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var friendClient = CreateAuthenticatedClient(friend);
        await friendClient.SendMessageAsync(chatId.Value, "one", CurrentCancellationToken);
        await friendClient.SendMessageAsync(chatId.Value, "two", CurrentCancellationToken);

        var response = await CreateAuthenticatedClient().GetChatsAsync(CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var chat = Assert.Single(response.Content!);
        Assert.Equal(chatId.Value, chat.ChatId);
        Assert.Equal(friend.Id.Value, chat.AssociatedUserId);
        Assert.Equal(2, chat.UnreadMessageCount);
    }

    [Fact]
    public async Task GetChats_Should_NotCountOwnMessagesAsUnread_When_UserSentThem()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();
        await client.SendMessageAsync(chatId.Value, "mine", CurrentCancellationToken);

        var response = await client.GetChatsAsync(CurrentCancellationToken);

        Assert.Equal(0, Assert.Single(response.Content!).UnreadMessageCount);
    }
}
