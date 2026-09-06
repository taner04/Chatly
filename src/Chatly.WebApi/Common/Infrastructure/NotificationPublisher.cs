using Chatly.Contracts.SignalR;
using Chatly.WebApi.Features.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Common.Infrastructure;

[SingletonService]
public sealed class NotificationPublisher(
    IHubContext<NotificationHub, INotificationHubClient> hubContext)
{
    public async Task PublishAsync(UserId receiverId, NotificationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        await hubContext.Clients.Group(NotificationHubGroups.User(receiverId)).Receive(message);
    }

    public Task PublishAsync(IEnumerable<UserId> receiverIds, NotificationMessage message)
    {
        ArgumentNullException.ThrowIfNull(receiverIds);
        ArgumentNullException.ThrowIfNull(message);

        return Task.WhenAll(receiverIds
            .Distinct()
            .Select(receiverId =>
                hubContext.Clients.Group(NotificationHubGroups.User(receiverId)).Receive(message)));
    }
}