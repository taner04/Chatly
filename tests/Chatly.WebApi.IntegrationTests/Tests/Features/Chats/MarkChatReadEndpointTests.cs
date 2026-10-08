namespace Chatly.WebApi.IntegrationTests.Tests.Features.Chats;

public sealed class MarkChatReadEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task MarkChatRead_Should_SucceedForEveryRequest_When_FirstReadsRunConcurrently()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();

        var responses = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(_ => client.MarkChatReadAsync(chatId.Value, CurrentCancellationToken)));

        responses.Should().AllSatisfy(response => response.StatusCode.Should().Be(HttpStatusCode.NoContent));
        await using var dbContext = GetDbContext();
        (await dbContext.ChatReadStates.CountAsync(CurrentCancellationToken)).Should().Be(1);
    }

    [Fact]
    public async Task MarkChatRead_Should_ResetUnreadCount_When_ChatIsRead()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        await CreateAuthenticatedClient(friend).SendMessageAsync(chatId.Value, "hello", CurrentCancellationToken);
        var client = CreateAuthenticatedClient();

        var response = await client.MarkChatReadAsync(chatId.Value, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var chats = await client.GetChatsAsync(CurrentCancellationToken);
        chats.Content!.Should().ContainSingle().Subject.UnreadMessageCount.Should().Be(0);
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
        chats.Content!.Should().ContainSingle().Subject.UnreadMessageCount.Should().Be(1);
    }
}