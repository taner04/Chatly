using Chatly.Contracts.Features.Hubs;
using Chatly.WebApi.Features.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Common.Infrastructure;

[SingletonService]
internal sealed class NotificationPublisher(
    IHubContext<NotificationHub, INotificationHubClient> hubContext)
{
    internal async Task PublishAsync(UserId receiverId, Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        await hubContext.Clients.Group(NotificationHubGroups.User(receiverId)).Receive(notification);
    }

    internal Task PublishAsync(IEnumerable<UserId> receiverIds, Notification notification)
    {
        ArgumentNullException.ThrowIfNull(receiverIds);
        ArgumentNullException.ThrowIfNull(notification);

        return Task.WhenAll(receiverIds
            .Distinct()
            .Select(receiverId =>
                hubContext.Clients.Group(NotificationHubGroups.User(receiverId)).Receive(notification)));
    }
}