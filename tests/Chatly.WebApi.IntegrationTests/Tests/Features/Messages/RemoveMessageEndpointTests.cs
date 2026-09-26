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

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await using var dbContext = GetDbContext();
        var message = await dbContext.Messages.SingleAsync(CurrentCancellationToken);
        Assert.True(message.IsDeleted);
    }

    [Fact]
    public async Task RemoveMessage_Should_Return404_When_OtherParticipantTriesToRemoveIt()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var sent = await CreateAuthenticatedClient().SendMessageAsync(chatId.Value, "mine", CurrentCancellationToken);

        var response = await CreateAuthenticatedClient(friend).RemoveMessageAsync(sent.Content!.MessageId, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await using var dbContext = GetDbContext();
        Assert.False((await dbContext.Messages.SingleAsync(CurrentCancellationToken)).IsDeleted);
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
        var blobUrl = Assert.Single(sent.Content!.Attachments).Url;
        Assert.Equal(HttpStatusCode.OK, await GetBlobStatusAsync(blobUrl));

        var response = await client.RemoveMessageAsync(sent.Content.MessageId, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await using var dbContext = GetDbContext();
        Assert.False(await dbContext.StoredFiles.AnyAsync(CurrentCancellationToken));
        Assert.False(await dbContext.MessageAttachments.AnyAsync(CurrentCancellationToken));
        Assert.Equal(HttpStatusCode.NotFound, await GetBlobStatusAsync(blobUrl));
    }
}
