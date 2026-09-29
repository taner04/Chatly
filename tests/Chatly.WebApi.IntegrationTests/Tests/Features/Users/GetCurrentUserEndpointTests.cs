namespace Chatly.WebApi.IntegrationTests.Tests.Features.Users;

public sealed class GetCurrentUserEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task GetCurrentUser_Should_Return401_When_Unauthenticated()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.GetCurrentUserAsync(CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_Should_ReturnExistingUser_When_Authenticated()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetCurrentUserAsync(CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(CurrentUser.Id.Value, response.Content!.UserId);
        Assert.Equal(CurrentUser.Username, response.Content.Username);
        Assert.True(response.Content.OnboardingCompleted);
    }

    [Fact]
    public async Task GetCurrentUser_Should_ProvisionUser_When_TokenBelongsToUnknownUser()
    {
        var unknown = new TestUser(UserId.From(Guid.NewGuid()), "auth0|unknown", "unknown@chatly.tests", "unknown");
        var client = CreateAuthenticatedClient(unknown);

        var response = await client.GetCurrentUserAsync(CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(unknown.Email, response.Content!.Email);
        Assert.Null(response.Content.Username);
        Assert.False(response.Content.OnboardingCompleted);

        await using var dbContext = GetDbContext();
        Assert.True(await dbContext.Users.AnyAsync(user => user.Auth0Id == unknown.Sub, CurrentCancellationToken));
    }
}