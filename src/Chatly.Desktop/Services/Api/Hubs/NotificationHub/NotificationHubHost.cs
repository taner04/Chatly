using Chatly.Desktop.Abstraction.Hubs;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub;

[SingletonService(typeof(IHubHost))]
internal sealed class NotificationHubHost(
    NotificationHubConnection connection,
    IHubMessageDispatcher dispatcher) : IHubHost
{
    private int _subscribed;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _subscribed, 1) == 0)
        {
            connection.On<NotificationMessage>(nameof(INotificationHubClient.Receive), dispatcher.DispatchAsync);
        }

        return connection.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => connection.StopAsync(cancellationToken);
}