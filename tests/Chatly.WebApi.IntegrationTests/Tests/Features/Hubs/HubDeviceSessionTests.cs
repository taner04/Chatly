using Microsoft.AspNetCore.SignalR.Client;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Hubs;

public sealed class HubDeviceSessionTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task Connect_Should_CreateDeviceSessionWithIdentitySession_When_DeviceConnectsFirstTime()
    {
        var deviceId = Guid.NewGuid();

        await using var device = await ConnectNotificationHubAsync(
            deviceId: deviceId,
            identitySessionId: "kc-hub-session");

        var session = await WaitForAsync(async () =>
        {
            await using var dbContext = GetDbContext();
            return await dbContext.DeviceSessions.SingleOrDefaultAsync(
                candidate => candidate.DeviceId == deviceId,
                CurrentCancellationToken);
        });
        session.Should().NotBeNull();
        session.UserId.Should().Be(CurrentUser.Id);
        session.IdentitySessionId.Should().Be("kc-hub-session");
        session.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task Connect_Should_CloseConnection_When_DeviceIdIsInvalid()
    {
        await using var device = await ConnectNotificationHubAsync(deviceId: Guid.Empty);

        var closed = await WaitUntilDisconnectedAsync(device.Connection);

        closed.Should().BeTrue();
        await using var dbContext = GetDbContext();
        (await dbContext.DeviceSessions.AnyAsync(CurrentCancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task ConnectCallHub_Should_CloseConnection_When_DeviceSessionIsRevoked()
    {
        var revokedDeviceId = Guid.NewGuid();
        await CreateDeviceSessionAsync(CurrentUser, revokedDeviceId, "laptop", DateTimeOffset.UtcNow);

        await using var device = await ConnectCallHubAsync(deviceId: revokedDeviceId);

        var closed = await WaitUntilDisconnectedAsync(device.Connection);

        closed.Should().BeTrue();
    }

    [Fact]
    public async Task Connect_Should_CloseConnectionAndNotCreateSession_When_IdentitySessionWasRevoked()
    {
        await CreateDeviceSessionAsync(
            CurrentUser,
            Guid.NewGuid(),
            "laptop",
            DateTimeOffset.UtcNow,
            "kc-revoked");
        var newDeviceId = Guid.NewGuid();

        await using var device = await ConnectNotificationHubAsync(
            deviceId: newDeviceId,
            identitySessionId: "kc-revoked");

        var closed = await WaitUntilDisconnectedAsync(device.Connection);

        closed.Should().BeTrue();
        await using var dbContext = GetDbContext();
        (await dbContext.DeviceSessions.AnyAsync(
            session => session.DeviceId == newDeviceId,
            CurrentCancellationToken)).Should().BeFalse();
    }

    private static async Task<bool> WaitUntilDisconnectedAsync(HubConnection connection) =>
        await WaitForAsync(() => Task.FromResult<object?>(
            connection.State == HubConnectionState.Disconnected ? true : null)) is not null;
}