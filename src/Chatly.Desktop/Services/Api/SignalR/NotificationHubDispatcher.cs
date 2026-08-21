using Chatly.Contracts.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Api.SignalR;

public sealed class NotificationHubDispatcher(
    ILogger<NotificationHubDispatcher> logger,
    IEnumerable<INotificationStrategy> notificationStrategies)
{
    public async Task DispatchAsync(NotificationMessage message)
    {
        var strategy = notificationStrategies.FirstOrDefault(s => s.Type == message.Type);

        if (strategy is null)
        {
            logger.LogWarning("No strategy found for notification type: {NotificationType}", message.Type);
            return;
        }
        
        await strategy.HandleNotificationAsync(message);
    }
}
