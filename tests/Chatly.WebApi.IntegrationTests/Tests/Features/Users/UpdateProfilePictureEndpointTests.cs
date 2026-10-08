namespace Chatly.WebApi.IntegrationTests.Tests.Features.Users;

public sealed class UpdateProfilePictureEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task UpdateProfilePicture_Should_StorePictureAndReturnUrl_When_ImageIsValid()
    {
        var client = CreateAuthenticatedClient();

        var response =
            await client.UpdateProfilePictureAsync(CreateFile("me.png", "image/png"), CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string.IsNullOrWhiteSpace(response.Content!.ProfilePictureUrl).Should().BeFalse();
        await using var dbContext = GetDbContext();
        var user = await dbContext.Users.SingleAsync(user => user.Id == CurrentUser.Id, CurrentCancellationToken);
        user.ProfilePictureFileId.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateProfilePicture_Should_Return400_When_FileIsNotAnImage()
    {
        var response = await CreateAuthenticatedClient().UpdateProfilePictureAsync(
            CreateFile("me.txt", "text/plain"),
            CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfilePicture_Should_DeletePreviousPicture_When_PictureIsReplaced()
    {
        var client = CreateAuthenticatedClient();
        var first = await client.UpdateProfilePictureAsync(CreateFile("first.png", "image/png"),
            CurrentCancellationToken);
        var previousUrl = first.Content!.ProfilePictureUrl!;

        var response =
            await client.UpdateProfilePictureAsync(CreateFile("second.png", "image/png"), CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.ProfilePictureUrl.Should().NotBe(previousUrl);
        await using var dbContext = GetDbContext();
        (await dbContext.StoredFiles.CountAsync(CurrentCancellationToken)).Should().Be(1);
        (await GetBlobStatusAsync(previousUrl)).Should().Be(HttpStatusCode.NotFound);
    }
}