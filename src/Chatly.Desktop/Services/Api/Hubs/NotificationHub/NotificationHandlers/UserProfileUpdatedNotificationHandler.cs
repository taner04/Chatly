using Chatly.Contracts.Features.Users.Notifications;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;

[SingletonService(typeof(IHubMessageHandler), true)]
internal sealed class UserProfileUpdatedNotificationHandler(
    UserRegistry userRegistry)
    : HubMessageHandler<UserProfileUpdatedNotification>
{
    protected override Task HandleMessageAsync(UserProfileUpdatedNotification message)
    {
        UiThreadDispatcher.SafeInvoke(() =>
        {
            userRegistry.UpdateProfile(message.UserId, message.Username, message.ProfilePictureUrl);
        });
        return Task.CompletedTask;
    }
}