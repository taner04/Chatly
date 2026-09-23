using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Calls;

namespace Chatly.Desktop.Services.Api.Hubs.CallHub;

[SingletonService(typeof(IHubHost))]
internal sealed class CallHubHost(
    CallHubConnection connection,
    IHubMessageDispatcher<Call> dispatcher,
    CallCoordinator coordinator) : IHubHost
{
    private IDisposable? _messageRegistration;
    private int _subscribed;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _subscribed, 1) == 0)
        {
            _messageRegistration = connection.On<Call>(nameof(ICallingHubClient.Receive), dispatcher.DispatchAsync);
            connection.Reconnected += OnReconnectedAsync;
        }

        return connection.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _subscribed, 0) != 0)
        {
            connection.Reconnected -= OnReconnectedAsync;
            _messageRegistration?.Dispose();
            _messageRegistration = null;
        }

        return connection.StopAsync(cancellationToken);
    }

    private Task OnReconnectedAsync() => coordinator.ReconcileAsync();
}
