using System.Net.Http.Headers;
using System.Security.Claims;
using Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.DeviceSession;
using Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Jwt;
using Refit;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Users;

public sealed class GetCurrentUserEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task GetCurrentUser_Should_Return401_When_Unauthenticated()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.GetCurrentUserAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_Should_ReturnExistingUser_When_Authenticated()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetCurrentUserAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.UserId.Should().Be(CurrentUser.Id.Value);
        response.Content.Username.Should().Be(CurrentUser.Username);
        response.Content.OnboardingCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentUser_Should_ProvisionUser_When_TokenBelongsToUnknownUser()
    {
        var unknown = new TestUser(UserId.From(Guid.NewGuid()), "identity|unknown", "unknown@chatly.tests", "unknown");
        var client = CreateAuthenticatedClient(unknown);

        var response = await client.GetCurrentUserAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Email.Should().Be(unknown.Email);
        response.Content.Username.Should().BeNull();
        response.Content.OnboardingCompleted.Should().BeFalse();

        await using var dbContext = GetDbContext();
        var provisioned =
            await dbContext.Users.AnyAsync(user => user.IdentityId == unknown.Sub, CurrentCancellationToken);
        provisioned.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentUser_Should_ProvisionUserOnce_When_UnknownUserSendsParallelRequests()
    {
        var unknown = new TestUser(UserId.From(Guid.NewGuid()), "identity|parallel", "parallel@chatly.tests",
            "parallel");
        var client = CreateAuthenticatedClient(unknown);

        var responses = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => client.GetCurrentUserAsync(CurrentCancellationToken)));

        responses.Should().AllSatisfy(response => response.StatusCode.Should().Be(HttpStatusCode.OK));
        responses.Select(response => response.Content!.UserId).Distinct().Should().ContainSingle();
        await using var dbContext = GetDbContext();
        var provisionedCount = await dbContext.Users.CountAsync(
            user => user.IdentityId == unknown.Sub,
            CurrentCancellationToken);
        provisionedCount.Should().Be(1);
    }

    [Fact]
    public async Task GetCurrentUser_Should_IgnoreUserIdClaimFromToken_When_TokenTriesToImpersonateAnotherUser()
    {
        var victim = await CreateUserAsync("victim");
        var httpClient = CreateHttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            JwtTokenMock.CreateToken(CurrentUser, new Claim("chatly_user_id", victim.Id.Value.ToString())));
        DeviceSessionHeadersMock.Apply(httpClient.DefaultRequestHeaders, Guid.NewGuid());

        var response = await RestService.For<IChatlyApiClient>(httpClient)
            .GetCurrentUserAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.UserId.Should().Be(CurrentUser.Id.Value);
    }
}