namespace Chatly.WebApi.IntegrationTests.Tests.Features.DeviceSessions;

public sealed class RevokeCurrentDeviceSessionEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task RevokeCurrentDeviceSession_Should_Return204AndRevokeOnlyCurrentSession_When_UserLogsOut()
    {
        var currentDeviceId = Guid.NewGuid();
        var currentSessionId = await CreateDeviceSessionAsync(CurrentUser, currentDeviceId, "current");
        var otherSessionId = await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "laptop");

        var response = await CreateAuthenticatedClient(deviceId: currentDeviceId)
            .RevokeCurrentDeviceSessionAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await using var dbContext = GetDbContext();
        var revokedAt = await dbContext.DeviceSessions
            .ToDictionaryAsync(session => session.Id, session => session.RevokedAt, CurrentCancellationToken);
        revokedAt[currentSessionId].Should().NotBeNull();
        revokedAt[otherSessionId].Should().BeNull();
    }

    [Fact]
    public async Task RevokeCurrentDeviceSession_Should_Return401_When_SessionIsUsedAfterLogout()
    {
        var currentDeviceId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(deviceId: currentDeviceId);
        await client.RevokeCurrentDeviceSessionAsync(CurrentCancellationToken);

        var response = await client.GetDeviceSessionsAsync(CurrentCancellationToken);

        AssertError(response, HttpStatusCode.Unauthorized, "DeviceSession.Revoked");
    }
}