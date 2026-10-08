using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Authentication;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub;

[SingletonService(typeof(IHubHost))]
internal sealed class NotificationHubHost(
    NotificationHubConnection connection,
    IHubMessageDispatcher dispatcher,
    SessionService sessionService) : IHubHost
{
    private readonly AtomicFlag _subscribed = new();
    private IDisposable? _messageRegistration;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_subscribed.TrySet())
        {
            _messageRegistration = connection.On<NotificationMessage>(
                nameof(INotificationHubClient.Receive),
                dispatcher.DispatchAsync);
            connection.Reconnected += OnReconnectedAsync;
        }

        return connection.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_subscribed.TryReset())
        {
            connection.Reconnected -= OnReconnectedAsync;
            _messageRegistration?.Dispose();
            _messageRegistration = null;
        }

        return connection.StopAsync(cancellationToken);
    }

    private Task OnReconnectedAsync() => sessionService.RefreshAsync(CancellationToken.None);
}