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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var chat = response.Content!.Should().ContainSingle().Subject;
        chat.ChatId.Should().Be(chatId.Value);
        chat.AssociatedUserId.Should().Be(friend.Id.Value);
        chat.UnreadMessageCount.Should().Be(2);
    }

    [Fact]
    public async Task GetChats_Should_NotCountOwnMessagesAsUnread_When_UserSentThem()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();
        await client.SendMessageAsync(chatId.Value, "mine", CurrentCancellationToken);

        var response = await client.GetChatsAsync(CurrentCancellationToken);

        response.Content!.Should().ContainSingle().Subject.UnreadMessageCount.Should().Be(0);
    }

    [Fact]
    public async Task GetChats_Should_NotCountDeletedMessagesAsUnread_When_FriendRemovedThem()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var friendClient = CreateAuthenticatedClient(friend);
        await friendClient.SendMessageAsync(chatId.Value, "kept", CurrentCancellationToken);
        var removed = await friendClient.SendMessageAsync(chatId.Value, "removed", CurrentCancellationToken);
        await friendClient.RemoveMessageAsync(removed.Content!.MessageId, CurrentCancellationToken);

        var response = await CreateAuthenticatedClient().GetChatsAsync(CurrentCancellationToken);

        response.Content!.Should().ContainSingle().Subject.UnreadMessageCount.Should().Be(1);
    }
}