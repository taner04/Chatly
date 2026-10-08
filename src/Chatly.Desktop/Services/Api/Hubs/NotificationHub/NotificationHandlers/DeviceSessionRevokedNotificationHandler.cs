using Chatly.Contracts.Features.DeviceSessions.Notifications;
using Chatly.Desktop.Abstraction.Hubs;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class DeviceSessionRevokedNotificationHandler(UserSessionContext sessionContext)
    : HubMessageHandler<DeviceSessionRevokedNotification>
{
    protected override Task HandleMessageAsync(DeviceSessionRevokedNotification message)
    {
        sessionContext.NotifyDeviceSessionRevoked();
        return Task.CompletedTask;
    }
}