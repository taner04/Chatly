using Chatly.Contracts.Features.Hubs;
using Chatly.WebApi.Features.DeviceSessions.Models;
using Chatly.WebApi.Features.Hubs;
using Chatly.WebApi.Features.Hubs.NotificationHub;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Common.Infrastructure;

[SingletonService]
public sealed class NotificationPublisher(
    IHubContext<NotificationHub, INotificationHubClient> hubContext)
{
    internal async Task PublishAsync(UserId receiverId, NotificationMessage notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        await hubContext.Clients.Group(HubGroups.User(receiverId)).Receive(notification);
    }

    internal Task PublishAsync(IEnumerable<DeviceSessionId> sessionIds, NotificationMessage notification)
    {
        ArgumentNullException.ThrowIfNull(sessionIds);
        ArgumentNullException.ThrowIfNull(notification);

        return Task.WhenAll(sessionIds
            .Distinct()
            .Select(sessionId =>
                hubContext.Clients.Group(HubGroups.DeviceSession(sessionId)).Receive(notification)));
    }

    internal Task PublishAsync(IEnumerable<UserId> receiverIds, NotificationMessage notification)
    {
        ArgumentNullException.ThrowIfNull(receiverIds);
        ArgumentNullException.ThrowIfNull(notification);

        return Task.WhenAll(receiverIds
            .Distinct()
            .Select(receiverId =>
                hubContext.Clients.Group(HubGroups.User(receiverId)).Receive(notification)));
    }
}