using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Calls;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Api.Hubs.CallHub;

[SingletonService(typeof(IHubHost))]
internal sealed class CallHubHost(
    CallHubConnection connection,
    IHubMessageDispatcher dispatcher,
    CallCoordinator coordinator) : IHubHost
{
    private readonly AtomicFlag _subscribed = new();
    private IDisposable? _messageRegistration;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_subscribed.TrySet())
        {
            _messageRegistration =
                connection.On<CallMessage>(nameof(ICallingHubClient.Receive), dispatcher.DispatchAsync);
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

    private Task OnReconnectedAsync() => coordinator.ReconcileAsync();
}