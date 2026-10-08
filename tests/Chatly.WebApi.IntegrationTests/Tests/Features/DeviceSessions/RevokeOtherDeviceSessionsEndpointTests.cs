namespace Chatly.WebApi.IntegrationTests.Tests.Features.DeviceSessions;

public sealed class RevokeOtherDeviceSessionsEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task RevokeOtherDeviceSessions_Should_Return204AndKeepCurrentSession_When_UserHasOtherSessions()
    {
        var currentDeviceId = Guid.NewGuid();
        var otherUser = await CreateUserAsync("other");
        var currentSessionId = await CreateDeviceSessionAsync(CurrentUser, currentDeviceId, "current");
        var laptopSessionId = await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "laptop");
        var desktopSessionId = await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "desktop");
        var foreignSessionId = await CreateDeviceSessionAsync(otherUser, Guid.NewGuid(), "foreign");

        var response = await CreateAuthenticatedClient(deviceId: currentDeviceId)
            .RevokeOtherDeviceSessionsAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await using var dbContext = GetDbContext();
        var revokedAt = await dbContext.DeviceSessions
            .ToDictionaryAsync(session => session.Id, session => session.RevokedAt, CurrentCancellationToken);
        revokedAt[currentSessionId].Should().BeNull();
        revokedAt[laptopSessionId].Should().NotBeNull();
        revokedAt[desktopSessionId].Should().NotBeNull();
        revokedAt[foreignSessionId].Should().BeNull();
    }

    [Fact]
    public async Task RevokeOtherDeviceSessions_Should_Return400_When_DeviceIdIsInvalid()
    {
        var response = await CreateAuthenticatedClient(deviceId: Guid.Empty)
            .RevokeOtherDeviceSessionsAsync(CurrentCancellationToken);

        AssertError(response, HttpStatusCode.BadRequest, "DeviceSession.HeaderInvalid");
    }
}