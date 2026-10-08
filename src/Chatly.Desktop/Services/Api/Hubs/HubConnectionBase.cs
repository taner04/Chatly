using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Options;
using Chatly.Desktop.Services.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Services.Api.Hubs;

internal abstract class HubConnectionBase(
    IOptions<WebApiClientOption> webApiClientOption,
    AuthenticationService authenticationService,
    AppSettings appSettings,
    ILogger logger) : IAsyncDisposable
{
    private const int MaxRetryAttempts = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);
    private readonly Lock _connectionLock = new();
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
    private readonly WebApiClientOption _webApiClientOption = webApiClientOption.Value;
    private bool _disposed;
    private HubConnection? _hubConnection;

    protected abstract string HubRoute { get; }

    public async ValueTask DisposeAsync()
    {
        await _lifecycleLock.WaitAsync();
        try
        {
            HubConnection? connection;
            lock (_connectionLock)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                connection = _hubConnection;
                _hubConnection = null;
            }

            if (connection is not null)
            {
                if (connection.State != HubConnectionState.Disconnected)
                {
                    await connection.StopAsync();
                }

                await connection.DisposeAsync();
            }
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    internal IDisposable On<TMessage>(string methodName, Func<TMessage, Task> handler) =>
        GetOrCreateConnection().On(methodName, handler);

    internal async Task StartAsync(CancellationToken cancellationToken)
    {
        await _lifecycleLock.WaitAsync(cancellationToken);
        try
        {
            var connection = GetOrCreateConnection();
            if (connection.State != HubConnectionState.Disconnected)
            {
                return;
            }

            for (var retryCount = 0; retryCount < MaxRetryAttempts; retryCount++)
            {
                try
                {
                    await connection.StartAsync(cancellationToken);
                    return;
                }
                catch (Exception exception) when (
                    exception is not OperationCanceledException && retryCount < MaxRetryAttempts - 1)
                {
                    logger.LogWarning(
                        exception,
                        "Failed to connect to the hub (attempt {RetryCount}).",
                        retryCount + 1);
                    await Task.Delay(RetryDelay, cancellationToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.LogError(
                        exception,
                        "Failed to connect to the hub after {MaxRetryAttempts} attempts.",
                        MaxRetryAttempts);
                    throw;
                }
            }
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    internal async Task StopAsync(CancellationToken cancellationToken)
    {
        await _lifecycleLock.WaitAsync(cancellationToken);
        try
        {
            HubConnection? connection;
            lock (_connectionLock)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                connection = _hubConnection;
            }

            if (connection is not null && connection.State != HubConnectionState.Disconnected)
            {
                await connection.StopAsync(cancellationToken);
            }
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    internal Task SendAsync(string methodName, params object?[] parameters)
    {
        HubConnection connection;
        lock (_connectionLock)
        {
            connection = _hubConnection
                         ?? throw new InvalidOperationException("The hub is not connected.");
            if (connection.State != HubConnectionState.Connected)
            {
                throw new InvalidOperationException("The hub is not connected.");
            }
        }

        return connection.SendAsync(methodName, parameters);
    }

    internal Task<TResult> InvokeAsync<TResult>(string methodName, params object?[] parameters)
    {
        HubConnection connection;
        lock (_connectionLock)
        {
            connection = _hubConnection
                         ?? throw new InvalidOperationException("The hub is not connected.");
            if (connection.State != HubConnectionState.Connected)
            {
                throw new InvalidOperationException("The hub is not connected.");
            }
        }

        return connection.InvokeCoreAsync<TResult>(methodName, parameters);
    }

    internal Task InvokeAsync(string methodName, params object?[] parameters)
    {
        HubConnection connection;
        lock (_connectionLock)
        {
            connection = _hubConnection
                         ?? throw new InvalidOperationException("The hub is not connected.");
            if (connection.State != HubConnectionState.Connected)
            {
                throw new InvalidOperationException("The hub is not connected.");
            }
        }

        return connection.InvokeCoreAsync(methodName, parameters);
    }

    internal event Func<Task>? Reconnected;

    private HubConnection GetOrCreateConnection()
    {
        lock (_connectionLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_hubConnection is not null)
            {
                return _hubConnection;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(
                    new Uri(_webApiClientOption.BaseAddress, HubRoute),
                    options =>
                    {
                        options.AccessTokenProvider = () =>
                            authenticationService.GetAccessTokenAsync(CancellationToken.None);
                        foreach (var (name, value) in DeviceSessionHeaderValues.Create(appSettings))
                        {
                            options.Headers[name] = value;
                        }
                    })
                .WithAutomaticReconnect(new IndefiniteRetryPolicy())
                .Build();
            _hubConnection.Reconnected += _ => NotifyReconnectedAsync();
            return _hubConnection;
        }
    }

    private async Task NotifyReconnectedAsync()
    {
        var handlers = Reconnected?.GetInvocationList();
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers)
        {
            await ((Func<Task>)handler)();
        }
    }
}