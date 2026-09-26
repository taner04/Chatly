namespace Chatly.WebApi.IntegrationTests.Tests.Features.Users;

public sealed class UpdateProfilePictureEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task UpdateProfilePicture_Should_StorePictureAndReturnUrl_When_ImageIsValid()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.UpdateProfilePictureAsync(CreateFile("me.png", "image/png"), CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(response.Content!.ProfilePictureUrl));
        await using var dbContext = GetDbContext();
        var user = await dbContext.Users.SingleAsync(user => user.Id == CurrentUser.Id, CurrentCancellationToken);
        Assert.NotNull(user.ProfilePictureFileId);
    }

    [Fact]
    public async Task UpdateProfilePicture_Should_Return400_When_FileIsNotAnImage()
    {
        var response = await CreateAuthenticatedClient().UpdateProfilePictureAsync(
            CreateFile("me.txt", "text/plain"),
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfilePicture_Should_DeletePreviousPicture_When_PictureIsReplaced()
    {
        var client = CreateAuthenticatedClient();
        var first = await client.UpdateProfilePictureAsync(CreateFile("first.png", "image/png"), CurrentCancellationToken);
        var previousUrl = first.Content!.ProfilePictureUrl!;

        var response = await client.UpdateProfilePictureAsync(CreateFile("second.png", "image/png"), CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(previousUrl, response.Content!.ProfilePictureUrl);
        await using var dbContext = GetDbContext();
        Assert.Equal(1, await dbContext.StoredFiles.CountAsync(CurrentCancellationToken));
        Assert.Equal(HttpStatusCode.NotFound, await GetBlobStatusAsync(previousUrl));
    }
}
