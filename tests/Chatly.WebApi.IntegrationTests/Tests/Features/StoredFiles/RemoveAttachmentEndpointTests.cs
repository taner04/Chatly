namespace Chatly.WebApi.IntegrationTests.Tests.Features.StoredFiles;

public sealed class RemoveAttachmentEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task RemoveAttachment_Should_Return204AndDeleteFile_When_OwnerRemovesUnusedFile()
    {
        var client = CreateAuthenticatedClient();
        var uploaded = await client.UploadAttachmentAsync(CreateFile("draft.txt", "text/plain"), CurrentCancellationToken);

        var response = await client.RemoveAttachmentAsync(uploaded.Content!.AttachmentId, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await using var dbContext = GetDbContext();
        Assert.False(await dbContext.StoredFiles.AnyAsync(CurrentCancellationToken));
    }

    [Fact]
    public async Task RemoveAttachment_Should_Return404_When_FileBelongsToAnotherUser()
    {
        var other = await CreateUserAsync("other");
        var uploaded = await CreateAuthenticatedClient(other).UploadAttachmentAsync(
            CreateFile("secret.txt", "text/plain"),
            CurrentCancellationToken);

        var response = await CreateAuthenticatedClient().RemoveAttachmentAsync(
            uploaded.Content!.AttachmentId,
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveAttachment_Should_Return404_When_FileIsAttachedToAMessage()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();
        var message = await client.SendMessageWithFilesAsync(
            chatId.Value,
            "attached",
            [CreateFile("kept.txt", "text/plain")],
            CurrentCancellationToken);

        var response = await client.RemoveAttachmentAsync(
            Assert.Single(message.Content!.Attachments).AttachmentId,
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
