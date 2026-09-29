namespace Chatly.WebApi.IntegrationTests.Tests.Features.Users;

public sealed class GetCurrentUserProfilePictureEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task GetCurrentUserProfilePicture_Should_ReturnNoUrl_When_UserHasNoPicture()
    {
        var response = await CreateAuthenticatedClient().GetCurrentUserProfilePictureAsync(CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.Content!.ProfilePictureUrl);
    }

    [Fact]
    public async Task GetCurrentUserProfilePicture_Should_ReturnUrl_When_UserUploadedPicture()
    {
        var client = CreateAuthenticatedClient();
        await client.UpdateProfilePictureAsync(CreateFile("me.webp", "image/webp"), CurrentCancellationToken);

        var response = await client.GetCurrentUserProfilePictureAsync(CurrentCancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(response.Content!.ProfilePictureUrl));
    }
}