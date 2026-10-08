namespace Chatly.WebApi.IntegrationTests.Tests.Features.DeviceSessions;

public sealed class RevokeDeviceSessionEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task RevokeDeviceSession_Should_Return204AndRevokeSession_When_SessionBelongsToOtherDevice()
    {
        var currentDeviceId = Guid.NewGuid();
        await CreateDeviceSessionAsync(CurrentUser, currentDeviceId, "current");
        var otherSessionId = await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "laptop");

        var response = await CreateAuthenticatedClient(deviceId: currentDeviceId)
            .RevokeDeviceSessionAsync(otherSessionId.Value, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await using var dbContext = GetDbContext();
        var session = await dbContext.DeviceSessions.SingleAsync(
            candidate => candidate.Id == otherSessionId,
            CurrentCancellationToken);
        session.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeDeviceSession_Should_Return409_When_SessionBelongsToCurrentDevice()
    {
        var currentDeviceId = Guid.NewGuid();
        var currentSessionId = await CreateDeviceSessionAsync(CurrentUser, currentDeviceId, "current");

        var response = await CreateAuthenticatedClient(deviceId: currentDeviceId)
            .RevokeDeviceSessionAsync(currentSessionId.Value, CurrentCancellationToken);

        AssertError(response, HttpStatusCode.Conflict, "DeviceSession.Current");
    }

    [Fact]
    public async Task RevokeDeviceSession_Should_Return404_When_SessionBelongsToOtherUser()
    {
        var otherUser = await CreateUserAsync("other");
        var foreignSessionId = await CreateDeviceSessionAsync(otherUser, Guid.NewGuid(), "foreign");

        var response = await CreateAuthenticatedClient(deviceId: Guid.NewGuid())
            .RevokeDeviceSessionAsync(foreignSessionId.Value, CurrentCancellationToken);

        AssertError(response, HttpStatusCode.NotFound, "DeviceSession.NotFound");
        await using var dbContext = GetDbContext();
        var session = await dbContext.DeviceSessions.SingleAsync(
            candidate => candidate.Id == foreignSessionId,
            CurrentCancellationToken);
        session.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task RevokeDeviceSession_Should_Return404_When_SessionIsAlreadyRevoked()
    {
        var revokedSessionId = await CreateDeviceSessionAsync(
            CurrentUser,
            Guid.NewGuid(),
            "revoked",
            DateTimeOffset.UtcNow);

        var response = await CreateAuthenticatedClient(deviceId: Guid.NewGuid())
            .RevokeDeviceSessionAsync(revokedSessionId.Value, CurrentCancellationToken);

        AssertError(response, HttpStatusCode.NotFound, "DeviceSession.NotFound");
    }
}