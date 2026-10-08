namespace Chatly.WebApi.IntegrationTests.Tests.Features.Messages;

public sealed class SendMessageEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task SendMessage_Should_ReturnMessageAndPersistIt_When_ChatBelongsToFriends()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);

        var response = await CreateAuthenticatedClient()
            .SendMessageAsync(chatId.Value, "  hello  ", CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Content.Should().Be("hello");
        response.Content.SenderUserId.Should().Be(CurrentUser.Id.Value);
        var messages = await CreateAuthenticatedClient(friend)
            .GetMessagesAsync(chatId.Value, 20, CurrentCancellationToken);
        messages.Content!.Items.Should().ContainSingle().Subject.MessageId.Should().Be(response.Content.MessageId);
    }

    [Fact]
    public async Task SendMessage_Should_Return404_When_UserIsNotPartOfTheChat()
    {
        var first = await CreateUserAsync("first");
        var second = await CreateUserAsync("second");
        var chatId = await CreateFriendshipAsync(first, second);

        var response = await CreateAuthenticatedClient()
            .SendMessageAsync(chatId.Value, "intruder", CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendMessage_Should_Return400_When_MessageIsEmpty()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);

        var response = await CreateAuthenticatedClient()
            .SendMessageAsync(chatId.Value, "   ", CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Attachments.Select(file => file.FileName).Order().Should().Equal("notes.txt", "photo.png");
        response.Content.Attachments.Should().AllSatisfy(attachment => attachment.Url.Should().NotBeNullOrWhiteSpace());
        await using var dbContext = GetDbContext();
        (await dbContext.MessageAttachments.CountAsync(CurrentCancellationToken)).Should().Be(2);
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

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await using var dbContext = GetDbContext();
        (await dbContext.Messages.AnyAsync(CurrentCancellationToken)).Should().BeFalse();
    }
}