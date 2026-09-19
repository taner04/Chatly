using Chatly.Contracts.Features.Users.Notifications;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
internal sealed class UserProfileUpdatedNotificationHandler(
    UserRegistry userRegistry,
    ILogger<ClientNotificationHandler<UserProfileUpdatedNotification>> logger)
    : ClientNotificationHandler<UserProfileUpdatedNotification>(logger)
{
    protected override Task HandleNotificationAsync(UserProfileUpdatedNotification message)
    {
        UIThreadDispatcher.SafeInvoke(() =>
        {
            userRegistry.UpdateProfile(message.UserId, message.Username, message.ProfilePictureUrl);
        });
        return Task.CompletedTask;
    }
}