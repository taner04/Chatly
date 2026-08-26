using System.Diagnostics;
using System.Threading.Tasks;
using Chatly.Contracts.FriendRequests.Results;
using Chatly.Contracts.SignalR;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR.NotificationEventStrategies;

public sealed class NotificationIncomingFriendRequestStrategy(
    ILogger<NotificationStrategy<FriendRequestResponse>> logger) : NotificationStrategy<FriendRequestResponse>(logger)
{
    public override NotificationType Type => NotificationType.IncomingFriendRequest;

    protected override Task HandleNotificationAsync(FriendRequestResponse message)
    {
        Debug.WriteLine($"Received incoming friend request from {message.SenderUserId}");
        return Task.CompletedTask;
    }
}