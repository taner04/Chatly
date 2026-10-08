using Chatly.Contracts.Features.DeviceSessions.Notifications;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.DeviceSessions;

public sealed class DeviceSessionsChangedNotificationTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task NewDevice_Should_NotifyUsersOtherDevices_When_DeviceSignsInForTheFirstTime()
    {
        await using var existingDevice = await ConnectNotificationHubAsync();

        var response = await CreateAuthenticatedClient(deviceId: Guid.NewGuid())
            .GetCurrentUserAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await existingDevice.ReceiveAsync<DeviceSessionsChangedNotification>();
    }

    [Fact]
    public async Task RevokeDeviceSession_Should_NotifyUsersDevices_When_SessionIsRevoked()
    {
        var currentDeviceId = Guid.NewGuid();
        var revokedSessionId = await CreateDeviceSessionAsync(CurrentUser, Guid.NewGuid(), "laptop");
        var client = CreateAuthenticatedClient(deviceId: currentDeviceId);
        await client.GetCurrentUserAsync(CurrentCancellationToken);
        await using var currentDevice = await ConnectNotificationHubAsync(deviceId: currentDeviceId);

        var response = await client.RevokeDeviceSessionAsync(revokedSessionId.Value, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await currentDevice.ReceiveAsync<DeviceSessionsChangedNotification>();
    }

    [Fact]
    public async Task NewDevice_Should_NotNotifyOtherUsers_When_DeviceSignsIn()
    {
        var otherUser = await CreateUserAsync("other");
        await using var otherUsersDevice = await ConnectNotificationHubAsync(otherUser);
        var currentDeviceId = Guid.NewGuid();
        await using var currentDevice = await ConnectNotificationHubAsync(deviceId: currentDeviceId);

        await CreateAuthenticatedClient(deviceId: Guid.NewGuid()).GetCurrentUserAsync(CurrentCancellationToken);
        await currentDevice.ReceiveAsync<DeviceSessionsChangedNotification>();

        otherUsersDevice.ReceivedSoFar<DeviceSessionsChangedNotification>().Should().BeEmpty();
    }
}