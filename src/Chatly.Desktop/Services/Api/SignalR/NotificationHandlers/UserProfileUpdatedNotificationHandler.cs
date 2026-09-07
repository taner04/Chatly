using Chatly.Contracts.SignalR;
using Chatly.Desktop.Utilities;
using Microsoft.Extensions.Logging;
using UserSessionContext = Chatly.Desktop.Models.UserSession.UserSessionContext;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
public sealed class UserProfileUpdatedNotificationHandler(
    UserSessionContext sessionContext,
    ILogger<ClientNotificationHandler<UserProfileUpdatedMessage>> logger)
    : ClientNotificationHandler<UserProfileUpdatedMessage>(logger)
{
    public override NotificationType Type => NotificationType.UserProfileUpdated;

    protected override Task HandleNotificationAsync(UserProfileUpdatedMessage message)
    {
        UiThreadDispatcher.SafeInvoke(() => sessionContext.UpdateUserProfile(
            message.UserId,
            message.Username,
            message.ProfilePictureUrl));
        return Task.CompletedTask;
    }
}