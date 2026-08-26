using Chatly.Contracts.SignalR;
using Chatly.WebApi.Features.Hubs;
using Chatly.WebApi.Features.Users.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Common.Infrastructure;

public sealed class NotificationPublisher(
    ILogger<NotificationPublisher> logger,
    IHubContext<NotificationHub, INotificationClient> hubContext)
{
    public async Task PublishAsync(UserId receiverId, NotificationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        await hubContext.Clients.Group(HubGroup.User(receiverId)).Receive(message);
    }
}