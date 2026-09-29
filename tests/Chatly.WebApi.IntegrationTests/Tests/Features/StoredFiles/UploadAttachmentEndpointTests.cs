namespace Chatly.WebApi.IntegrationTests.Tests.Features.StoredFiles;

public sealed class UploadAttachmentEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task UploadAttachment_Should_StoreFileForCurrentUser_When_FileIsValid()
    {
        var response = await CreateAuthenticatedClient().UploadAttachmentAsync(
            CreateFile("report.pdf", "application/pdf", 128),
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("report.pdf", response.Content!.FileName);
        Assert.Equal(128, response.Content.Size);
        await using var dbContext = GetDbContext();
        var storedFile = await dbContext.StoredFiles.SingleAsync(CurrentCancellationToken);
        Assert.Equal(CurrentUser.Id, storedFile.UserId);
    }

    [Fact]
    public async Task UploadAttachment_Should_Return400_When_FileIsEmpty()
    {
        var response = await CreateAuthenticatedClient().UploadAttachmentAsync(
            CreateFile("empty.txt", "text/plain", 0),
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadAttachment_Should_Return401_When_Unauthenticated()
    {
        var response = await CreateUnauthenticatedClient().UploadAttachmentAsync(
            CreateFile("report.pdf", "application/pdf"),
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}