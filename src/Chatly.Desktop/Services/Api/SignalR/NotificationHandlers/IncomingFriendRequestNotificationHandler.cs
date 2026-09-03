using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.SignalR;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Mappers;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationHandlers;

[SingletonService(typeof(IClientNotificationHandler))]
public sealed class IncomingFriendRequestNotificationHandler(
    UserSessionContext userSessionContext,
    IToastService toastService,
    ILogger<ClientNotificationHandler<IncomingFriendRequestMessage>> logger)
    : ClientNotificationHandler<IncomingFriendRequestMessage>(logger)
{
    public override NotificationType Type => NotificationType.IncomingFriendRequest;

    protected override Task HandleNotificationAsync(IncomingFriendRequestMessage message)
    {
        var friendRequest = FriendRequestMapper.Map(message);
        if (userSessionContext.AddFriendRequest(friendRequest))
        {
            toastService.AddNotification($"Friend request from '{message.SenderUsername}'");
        }

        return Task.CompletedTask;
    }
}