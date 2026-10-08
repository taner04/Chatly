using Chatly.Contracts.Features.DeviceSessions.Notifications;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Utilities;
using Chatly.Desktop.ViewModels.Pages;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler))]
internal sealed class DeviceSessionsChangedNotificationHandler(SettingsPageViewModel settingsPageViewModel)
    : HubMessageHandler<DeviceSessionsChangedNotification>
{
    protected override Task HandleMessageAsync(DeviceSessionsChangedNotification message) =>
        UiThreadDispatcher.SafeInvokeAsync(() =>
            settingsPageViewModel.RefreshDeviceSessionsCommand.CanExecute(null)
                ? settingsPageViewModel.RefreshDeviceSessionsCommand.ExecuteAsync(null)
                : Task.CompletedTask);
}