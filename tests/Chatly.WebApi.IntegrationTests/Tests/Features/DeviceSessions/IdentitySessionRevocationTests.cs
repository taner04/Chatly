using System.Text.Json;
using Chatly.Contracts.Features.DeviceSessions.Notifications;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.DeviceSessions;

public sealed class IdentitySessionRevocationTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task Request_Should_StoreIdentitySessionId_When_TokenContainsSessionId()
    {
        var deviceId = Guid.NewGuid();

        var response = await CreateAuthenticatedClient(deviceId: deviceId, identitySessionId: "kc-session-1")
            .GetCurrentUserAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var dbContext = GetDbContext();
        var session = await dbContext.DeviceSessions.SingleAsync(
            candidate => candidate.DeviceId == deviceId,
            CurrentCancellationToken);
        session.IdentitySessionId.Should().Be("kc-session-1");
    }

    [Fact]
    public async Task Request_Should_Return401AndNotCreateSession_When_IdentitySessionWasRevokedOnAnotherDeviceId()
    {
        await CreateDeviceSessionAsync(
            CurrentUser,
            Guid.NewGuid(),
            "laptop",
            DateTimeOffset.UtcNow,
            "kc-revoked");
        var newDeviceId = Guid.NewGuid();

        var response = await CreateAuthenticatedClient(deviceId: newDeviceId, identitySessionId: "kc-revoked")
            .GetDeviceSessionsAsync(CurrentCancellationToken);

        AssertError(response, HttpStatusCode.Unauthorized, "DeviceSession.Revoked");
        await using var dbContext = GetDbContext();
        (await dbContext.DeviceSessions.AnyAsync(
            session => session.DeviceId == newDeviceId,
            CurrentCancellationToken)).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(IdentityProviderFailures))]
    public async Task RevokeDeviceSession_Should_RevokeAndNotifyDevice_When_IdentityProviderFails(Exception failure)
    {
        var currentDeviceId = Guid.NewGuid();
        var revokedDeviceId = Guid.NewGuid();
        var revokedSessionId = await CreateDeviceSessionAsync(
            CurrentUser,
            revokedDeviceId,
            "laptop",
            identitySessionId: "kc-laptop");
        await using var revokedDevice = await ConnectNotificationHubAsync(deviceId: revokedDeviceId);
        IdentityProvider.FailWith(failure);

        var response = await CreateAuthenticatedClient(deviceId: currentDeviceId)
            .RevokeDeviceSessionAsync(revokedSessionId.Value, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await revokedDevice.ReceiveAsync<DeviceSessionRevokedNotification>();
        await using var dbContext = GetDbContext();
        var session = await dbContext.DeviceSessions.SingleAsync(
            candidate => candidate.Id == revokedSessionId,
            CurrentCancellationToken);
        session.RevokedAt.Should().NotBeNull();
    }

    public static TheoryData<Exception> IdentityProviderFailures() =>
    [
        new HttpRequestException("unreachable"),
        new TaskCanceledException("timeout"),
        new JsonException("invalid json")
    ];

    [Fact]
    public async Task RevokeDeviceSession_Should_EndIdentitySession_When_SessionIsRevoked()
    {
        var currentDeviceId = Guid.NewGuid();
        await CreateDeviceSessionAsync(CurrentUser, currentDeviceId, "current", identitySessionId: "kc-current");
        var otherSessionId = await CreateDeviceSessionAsync(
            CurrentUser,
            Guid.NewGuid(),
            "laptop",
            identitySessionId: "kc-laptop");

        var response = await CreateAuthenticatedClient(deviceId: currentDeviceId)
            .RevokeDeviceSessionAsync(otherSessionId.Value, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        IdentityProvider.RevokedSessionIds.Should().Equal("kc-laptop");
    }

    [Fact]
    public async Task RevokeOtherDeviceSessions_Should_EndOtherIdentitySessions_When_UserSignsOutOtherDevices()
    {
        var currentDeviceId = Guid.NewGuid();
        await CreateDeviceSessionAsync(CurrentUser, currentDeviceId, "current", identitySessionId: "kc-current");
        await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "laptop", identitySessionId: "kc-laptop");
        await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "desktop", identitySessionId: "kc-desktop");

        var response = await CreateAuthenticatedClient(deviceId: currentDeviceId)
            .RevokeOtherDeviceSessionsAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        IdentityProvider.RevokedSessionIds.Should().BeEquivalentTo("kc-laptop", "kc-desktop");
    }

    [Fact]
    public async Task RevokeCurrentDeviceSession_Should_EndOwnIdentitySession_When_UserLogsOut()
    {
        var currentDeviceId = Guid.NewGuid();
        await CreateDeviceSessionAsync(CurrentUser, currentDeviceId, "current", identitySessionId: "kc-current");

        var response = await CreateAuthenticatedClient(deviceId: currentDeviceId)
            .RevokeCurrentDeviceSessionAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        IdentityProvider.RevokedSessionIds.Should().Equal("kc-current");
    }
}