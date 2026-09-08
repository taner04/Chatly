namespace Chatly.Desktop.Services.Api.SignalR;

[SingletonService(typeof(INotificationHubServer))]
internal sealed class NotificationHubServerProxy(NotificationHubConnection notificationHubConnection)
    : INotificationHubServer
{
    public Task IsOnline(Guid userId)
    {
        return notificationHubConnection.SendAsync(nameof(INotificationHubServer.IsOnline), userId);
    }

    public Task IsOffline(Guid userId)
    {
        return notificationHubConnection.SendAsync(nameof(INotificationHubServer.IsOffline), userId);
    }

    public Task StartTyping(Guid chatId)
    {
        return notificationHubConnection.SendAsync(nameof(INotificationHubServer.StartTyping), chatId);
    }

    public Task StopTyping(Guid chatId)
    {
        return notificationHubConnection.SendAsync(nameof(INotificationHubServer.StopTyping), chatId);
    }
}
