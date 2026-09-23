using Chatly.Contracts.Features.Users.Notifications;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler<Notification>), true)]
internal sealed class UserProfileUpdatedNotificationHandler(
    UserRegistry userRegistry,
    ILogger<NotificationHandler<UserProfileUpdatedNotification>> logger)
    : NotificationHandler<UserProfileUpdatedNotification>(logger)
{
    protected override Task HandleNotificationAsync(UserProfileUpdatedNotification message)
    {
        UiThreadDispatcher.SafeInvoke(() =>
        {
            userRegistry.UpdateProfile(message.UserId, message.Username, message.ProfilePictureUrl);
        });
        return Task.CompletedTask;
    }
}