namespace Chatly.WebApi.IntegrationTests.Tests.Features.Messages;

public sealed class GetMessagesEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task GetMessages_Should_PageFromNewestToOldest_When_MoreMessagesThanPageSize()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();
        foreach (var text in new[] { "first", "second", "third" })
        {
            await client.SendMessageAsync(chatId.Value, text, CurrentCancellationToken);
        }

        var firstPage = await client.GetMessagesAsync(chatId.Value, 2, CurrentCancellationToken);
        var secondPage = await client.GetMessagesBeforeAsync(
            chatId.Value,
            firstPage.Content!.NextBeforeSentAt!.Value,
            firstPage.Content.NextBeforeMessageId!.Value,
            2,
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstPage.StatusCode);
        Assert.True(firstPage.Content.HasMore);
        Assert.Equal(["second", "third"], firstPage.Content.Items.Select(message => message.Content));
        Assert.False(secondPage.Content!.HasMore);
        Assert.Equal("first", Assert.Single(secondPage.Content.Items).Content);
    }

    [Fact]
    public async Task GetMessages_Should_Return404_When_UserIsNotPartOfTheChat()
    {
        var first = await CreateUserAsync("first");
        var second = await CreateUserAsync("second");
        var chatId = await CreateFriendshipAsync(first, second);

        var response = await CreateAuthenticatedClient().GetMessagesAsync(chatId.Value, 20, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_Should_MarkRemovedMessagesAsDeleted_When_SenderRemovedThem()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();
        var sent = await client.SendMessageAsync(chatId.Value, "secret", CurrentCancellationToken);
        await client.RemoveMessageAsync(sent.Content!.MessageId, CurrentCancellationToken);

        var response = await CreateAuthenticatedClient(friend)
            .GetMessagesAsync(chatId.Value, 20, CurrentCancellationToken);

        var message = Assert.Single(response.Content!.Items);
        Assert.True(message.IsDeleted);
        Assert.DoesNotContain("secret", message.Content);
    }
}