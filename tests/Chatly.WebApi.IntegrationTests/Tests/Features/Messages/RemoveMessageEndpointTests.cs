namespace Chatly.WebApi.IntegrationTests.Tests.Features.Messages;

public sealed class RemoveMessageEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task RemoveMessage_Should_Return204AndMarkMessageDeleted_When_SenderRemovesIt()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();
        var sent = await client.SendMessageAsync(chatId.Value, "oops", CurrentCancellationToken);

        var response = await client.RemoveMessageAsync(sent.Content!.MessageId, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await using var dbContext = GetDbContext();
        var message = await dbContext.Messages.SingleAsync(CurrentCancellationToken);
        message.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveMessage_Should_Return404_When_OtherParticipantTriesToRemoveIt()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var sent = await CreateAuthenticatedClient().SendMessageAsync(chatId.Value, "mine", CurrentCancellationToken);

        var response = await CreateAuthenticatedClient(friend)
            .RemoveMessageAsync(sent.Content!.MessageId, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await using var dbContext = GetDbContext();
        (await dbContext.Messages.SingleAsync(CurrentCancellationToken)).IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveMessage_Should_DeleteAttachmentsAndBlobs_When_MessageHasFiles()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();
        var sent = await client.SendMessageWithFilesAsync(
            chatId.Value,
            "with file",
            [CreateFile("notes.txt", "text/plain")],
            CurrentCancellationToken);
        var blobUrl = sent.Content!.Attachments.Should().ContainSingle().Subject.Url;
        (await GetBlobStatusAsync(blobUrl)).Should().Be(HttpStatusCode.OK);

        var response = await client.RemoveMessageAsync(sent.Content.MessageId, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await using var dbContext = GetDbContext();
        (await dbContext.StoredFiles.AnyAsync(CurrentCancellationToken)).Should().BeFalse();
        (await dbContext.MessageAttachments.AnyAsync(CurrentCancellationToken)).Should().BeFalse();
        (await GetBlobStatusAsync(blobUrl)).Should().Be(HttpStatusCode.NotFound);
    }
}