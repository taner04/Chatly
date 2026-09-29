namespace Chatly.WebApi.IntegrationTests.Tests.Features.Chats;

public sealed class MarkChatReadEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task MarkChatRead_Should_ResetUnreadCount_When_ChatIsRead()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        await CreateAuthenticatedClient(friend).SendMessageAsync(chatId.Value, "hello", CurrentCancellationToken);
        var client = CreateAuthenticatedClient();

        var response = await client.MarkChatReadAsync(chatId.Value, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var chats = await client.GetChatsAsync(CurrentCancellationToken);
        Assert.Equal(0, Assert.Single(chats.Content!).UnreadMessageCount);
    }

    [Fact]
    public async Task MarkChatRead_Should_Return403_When_UserIsNotPartOfTheChat()
    {
        var first = await CreateUserAsync("first");
        var second = await CreateUserAsync("second");
        var chatId = await CreateFriendshipAsync(first, second);

        var response = await CreateAuthenticatedClient().MarkChatReadAsync(chatId.Value, CurrentCancellationToken);

        AssertError(response, HttpStatusCode.Forbidden, "Chat.AccessDenied");
    }

    [Fact]
    public async Task MarkChatRead_Should_CountOnlyNewerMessages_When_FriendWritesAfterRead()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var friendClient = CreateAuthenticatedClient(friend);
        await friendClient.SendMessageAsync(chatId.Value, "before", CurrentCancellationToken);
        var client = CreateAuthenticatedClient();
        await client.MarkChatReadAsync(chatId.Value, CurrentCancellationToken);

        await friendClient.SendMessageAsync(chatId.Value, "after", CurrentCancellationToken);

        var chats = await client.GetChatsAsync(CurrentCancellationToken);
        Assert.Equal(1, Assert.Single(chats.Content!).UnreadMessageCount);
    }
}