namespace Chatly.WebApi.IntegrationTests.Tests.Features.Users;

public sealed class GetCurrentUserProfilePictureEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task GetCurrentUserProfilePicture_Should_ReturnNoUrl_When_UserHasNoPicture()
    {
        var response = await CreateAuthenticatedClient().GetCurrentUserProfilePictureAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.ProfilePictureUrl.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserProfilePicture_Should_ReturnUrl_When_UserUploadedPicture()
    {
        var client = CreateAuthenticatedClient();
        await client.UpdateProfilePictureAsync(CreateFile("me.webp", "image/webp"), CurrentCancellationToken);

        var response = await client.GetCurrentUserProfilePictureAsync(CurrentCancellationToken);

        string.IsNullOrWhiteSpace(response.Content!.ProfilePictureUrl).Should().BeFalse();
    }
}