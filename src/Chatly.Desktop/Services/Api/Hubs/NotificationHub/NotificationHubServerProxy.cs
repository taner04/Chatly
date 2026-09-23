namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub;

[SingletonService(typeof(INotificationHubServer))]
internal sealed class NotificationHubServerProxy(NotificationHubConnection connection)
    : INotificationHubServer
{
    public Task StartTyping(Guid chatId) =>
        connection.SendAsync(nameof(INotificationHubServer.StartTyping), chatId);

    public Task StopTyping(Guid chatId) =>
        connection.SendAsync(nameof(INotificationHubServer.StopTyping), chatId);
}