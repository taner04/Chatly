using Chatly.Contracts.Features.DeviceSessions.Notifications;
using Chatly.Desktop.Models.UserSession;
using Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

namespace Chatly.Desktop.UnitTests.Tests.Services.Hubs;

public sealed class DeviceSessionRevokedNotificationHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_RaiseDeviceSessionRevoked_When_DeviceWasSignedOut()
    {
        var sessionContext = new UserSessionContext(new UserRegistry());
        var raised = false;
        sessionContext.DeviceSessionRevoked += (_, _) => raised = true;

        await new DeviceSessionRevokedNotificationHandler(sessionContext)
            .HandleAsync(new DeviceSessionRevokedNotification());

        raised.Should().BeTrue();
    }
}