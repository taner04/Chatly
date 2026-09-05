using Chatly.Contracts.SignalR;
using Chatly.Desktop.Options;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserSessionContext = Chatly.Desktop.Models.UserSession.UserSessionContext;

namespace Chatly.Desktop.Services.Api.SignalR;

[SingletonService]
public sealed partial class NotificationHubConnection(
    IOptions<WebApiClientOption> options,
    UserSessionContext sessionContext,
    ILogger<NotificationHubConnection> logger) : IAsyncDisposable
{
    private const int MaxRetryAttempts = 5;
    private readonly WebApiClientOption _webApiClientOption = options.Value;
    private HubConnection? _hubConnection;

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    public async Task StartHubAsync(Func<NotificationMessage, Task> dispatchAsync)
    {
        if (_hubConnection is not null)
        {
            return;
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_webApiClientOption.HubAddress,
                options => { options.AccessTokenProvider = () => Task.FromResult(sessionContext.AccessToken); })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On("Receive", dispatchAsync);

        var retryCount = 0;
        while (retryCount < MaxRetryAttempts)
        {
            try
            {
                await _hubConnection.StartAsync();
                break;
            }
            catch (Exception exception)
            {
                if (retryCount >= MaxRetryAttempts - 1)
                {
                    LogConnectionFailed(MaxRetryAttempts, exception);
                    throw;
                }

                LogConnectionRetry(retryCount + 1, exception);
                retryCount++;
            }
        }
    }

    public Task SendAsync(string methodName, Guid chatId)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("The notification hub is not connected.");
        }

        return _hubConnection.SendAsync(methodName, chatId);
    }

    [LoggerMessage(
        LogLevel.Error,
        "Failed to connect to the notification hub after {MaxRetryAttempts} attempts.")]
    private partial void LogConnectionFailed(int maxRetryAttempts, Exception exception);

    [LoggerMessage(
        LogLevel.Warning,
        "Failed to connect to the notification hub (attempt {RetryCount}).")]
    private partial void LogConnectionRetry(int retryCount, Exception exception);
}