namespace Chatly.WebApi.IntegrationTests.Tests.Features.Messages;

public sealed class SendMessageEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task SendMessage_Should_ReturnMessageAndPersistIt_When_ChatBelongsToFriends()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);

        var response = await CreateAuthenticatedClient().SendMessageAsync(chatId.Value, "  hello  ", CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("hello", response.Content!.Content);
        Assert.Equal(CurrentUser.Id.Value, response.Content.SenderUserId);
        var messages = await CreateAuthenticatedClient(friend).GetMessagesAsync(chatId.Value, 20, CurrentCancellationToken);
        Assert.Equal(response.Content.MessageId, Assert.Single(messages.Content!.Items).MessageId);
    }

    [Fact]
    public async Task SendMessage_Should_Return404_When_UserIsNotPartOfTheChat()
    {
        var first = await CreateUserAsync("first");
        var second = await CreateUserAsync("second");
        var chatId = await CreateFriendshipAsync(first, second);

        var response = await CreateAuthenticatedClient().SendMessageAsync(chatId.Value, "intruder", CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_Should_Return400_When_MessageIsEmpty()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);

        var response = await CreateAuthenticatedClient().SendMessageAsync(chatId.Value, "   ", CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_Should_StoreAttachments_When_FilesAreSent()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);

        var response = await CreateAuthenticatedClient().SendMessageWithFilesAsync(
            chatId.Value,
            "see attached",
            [CreateFile("notes.txt", "text/plain"), CreateFile("photo.png", "image/png")],
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["notes.txt", "photo.png"], response.Content!.Attachments.Select(file => file.FileName).Order());
        Assert.All(response.Content.Attachments, attachment => Assert.False(string.IsNullOrWhiteSpace(attachment.Url)));
        await using var dbContext = GetDbContext();
        Assert.Equal(2, await dbContext.MessageAttachments.CountAsync(CurrentCancellationToken));
    }

    [Fact]
    public async Task SendMessage_Should_Return400_When_TooManyFilesAreAttached()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);

        var response = await CreateAuthenticatedClient().SendMessageWithFilesAsync(
            chatId.Value,
            "too many",
            Enumerable.Range(1, 6).Select(index => CreateFile($"file{index}.txt", "text/plain")),
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await using var dbContext = GetDbContext();
        Assert.False(await dbContext.Messages.AnyAsync(CurrentCancellationToken));
    }
}
