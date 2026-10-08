using Chatly.Contracts.Features.DeviceSessions.Notifications;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.DeviceSessions;

public sealed class DeviceSessionRevokedNotificationTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task RevokeDeviceSession_Should_NotifyOnlyRevokedDevice_When_DeviceIsConnected()
    {
        var revokedDeviceId = Guid.NewGuid();
        var keptDeviceId = Guid.NewGuid();
        var revokedSessionId = await CreateDeviceSessionAsync(CurrentUser, revokedDeviceId, "laptop");
        await using var revokedDevice = await ConnectNotificationHubAsync(deviceId: revokedDeviceId);
        await using var keptDevice = await ConnectNotificationHubAsync(deviceId: keptDeviceId);

        var response = await CreateAuthenticatedClient(deviceId: keptDeviceId)
            .RevokeDeviceSessionAsync(revokedSessionId.Value, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await revokedDevice.ReceiveAsync<DeviceSessionRevokedNotification>();
        keptDevice.ReceivedSoFar<DeviceSessionRevokedNotification>().Should().BeEmpty();
    }

    [Fact]
    public async Task RevokeOtherDeviceSessions_Should_NotifyEveryOtherDevice_When_DevicesAreConnected()
    {
        var currentDeviceId = Guid.NewGuid();
        await using var first = await ConnectNotificationHubAsync(deviceId: Guid.NewGuid());
        await using var second = await ConnectNotificationHubAsync(deviceId: Guid.NewGuid());

        var response = await CreateAuthenticatedClient(deviceId: currentDeviceId)
            .RevokeOtherDeviceSessionsAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await first.ReceiveAsync<DeviceSessionRevokedNotification>();
        await second.ReceiveAsync<DeviceSessionRevokedNotification>();
    }

    [Fact]
    public async Task Connect_Should_CloseConnection_When_DeviceSessionIsRevoked()
    {
        var revokedDeviceId = Guid.NewGuid();
        await CreateDeviceSessionAsync(CurrentUser, revokedDeviceId, "laptop", DateTimeOffset.UtcNow);

        await using var revokedDevice = await ConnectNotificationHubAsync(deviceId: revokedDeviceId);

        var closed = await WaitForAsync(() => Task.FromResult<object?>(
            revokedDevice.Connection.State == HubConnectionState.Disconnected ? true : null));
        closed.Should().NotBeNull();
    }
}