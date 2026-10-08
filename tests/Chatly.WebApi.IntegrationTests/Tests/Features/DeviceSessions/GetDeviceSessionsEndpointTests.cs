namespace Chatly.WebApi.IntegrationTests.Tests.Features.DeviceSessions;

public sealed class GetDeviceSessionsEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task GetDeviceSessions_Should_ReturnActiveSessionsAndMarkCurrent_When_UserHasSessions()
    {
        var currentDeviceId = Guid.NewGuid();
        var otherUser = await CreateUserAsync("other");
        var currentSessionId = await CreateDeviceSessionAsync(CurrentUser, currentDeviceId, "current");
        var otherSessionId = await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "laptop");
        await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "revoked", DateTimeOffset.UtcNow);
        await CreateDeviceSessionAsync(otherUser, Guid.NewGuid(), "foreign");

        var response = await CreateAuthenticatedClient(deviceId: currentDeviceId)
            .GetDeviceSessionsAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessions = response.Content!;
        sessions.Count.Should().Be(2);
        sessions.Single(session => session.SessionId == currentSessionId.Value).IsCurrent.Should().BeTrue();
        sessions.Single(session => session.SessionId == otherSessionId.Value).IsCurrent.Should().BeFalse();
    }

    [Fact]
    public async Task GetDeviceSessions_Should_Return400_When_DeviceIdIsInvalid()
    {
        var response = await CreateAuthenticatedClient(deviceId: Guid.Empty)
            .GetDeviceSessionsAsync(CurrentCancellationToken);

        AssertError(response, HttpStatusCode.BadRequest, "DeviceSession.HeaderInvalid");
    }
}