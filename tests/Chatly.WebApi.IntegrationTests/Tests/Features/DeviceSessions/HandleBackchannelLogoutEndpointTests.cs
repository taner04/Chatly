using Chatly.Contracts.Features.DeviceSessions.Notifications;
using Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Jwt;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.DeviceSessions;

public sealed class HandleBackchannelLogoutEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task BackchannelLogout_Should_RevokeSessionAndNotifyDevice_When_IdentityProviderEndsSession()
    {
        var deviceId = Guid.NewGuid();
        var sessionId = await CreateDeviceSessionAsync(CurrentUser, deviceId, "laptop", identitySessionId: "kc-laptop");
        await using var device = await ConnectNotificationHubAsync(deviceId: deviceId);

        using var response = await PostLogoutTokenAsync(JwtTokenMock.CreateLogoutToken("kc-laptop"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await device.ReceiveAsync<DeviceSessionRevokedNotification>();
        await using var dbContext = GetDbContext();
        var session = await dbContext.DeviceSessions.SingleAsync(
            candidate => candidate.Id == sessionId,
            CurrentCancellationToken);
        session.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task BackchannelLogout_Should_Return400AndKeepSession_When_AudienceIsWrong()
    {
        var sessionId = await CreateDeviceSessionAsync(
            CurrentUser,
            Guid.NewGuid(),
            "laptop",
            identitySessionId: "kc-laptop");

        using var response = await PostLogoutTokenAsync(
            JwtTokenMock.CreateLogoutToken("kc-laptop", "another-client"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await using var dbContext = GetDbContext();
        var session = await dbContext.DeviceSessions.SingleAsync(
            candidate => candidate.Id == sessionId,
            CurrentCancellationToken);
        session.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task BackchannelLogout_Should_Return400_When_TokenIsNotALogoutToken()
    {
        using var response = await PostLogoutTokenAsync(
            JwtTokenMock.CreateLogoutToken("kc-laptop", includeLogoutEvent: false));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private Task<HttpResponseMessage> PostLogoutTokenAsync(string logoutToken) =>
        CreateHttpClient().PostAsync(
            ApiRoutes.Identity.BackchannelLogout,
            new FormUrlEncodedContent(new Dictionary<string, string> { ["logout_token"] = logoutToken }),
            CurrentCancellationToken);
}