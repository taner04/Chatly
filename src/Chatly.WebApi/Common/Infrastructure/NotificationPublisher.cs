using Chatly.Contracts.SignalR;
using Chatly.WebApi.Features.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Common.Infrastructure;

[SingletonService]
internal sealed class NotificationPublisher(
    IHubContext<NotificationHub, INotificationHubClient> hubContext)
{
    internal async Task PublishAsync(UserId receiverId, NotificationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        await hubContext.Clients.Group(NotificationHubGroups.User(receiverId)).Receive(message);
    }

    internal Task PublishAsync(IEnumerable<UserId> receiverIds, NotificationMessage message)
    {
        ArgumentNullException.ThrowIfNull(receiverIds);
        ArgumentNullException.ThrowIfNull(message);

        return Task.WhenAll(receiverIds
            .Distinct()
            .Select(receiverId =>
                hubContext.Clients.Group(NotificationHubGroups.User(receiverId)).Receive(message)));
    }
}
