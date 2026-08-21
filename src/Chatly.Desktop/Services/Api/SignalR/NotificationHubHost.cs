using Chatly.Contracts.SignalR;
using Chatly.Desktop.Models;
using Chatly.Desktop.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chatly.Desktop.Services.Api.SignalR;

public sealed class NotificationHubHost(
    IOptions<WebApiClientOption> options,
    UserSessionContext sessionContext,
    NotificationHubDispatcher notificationHubDispatcher,
    ILogger<NotificationHubHost> logger) : IAsyncDisposable
{
    private readonly WebApiClientOption _webApiClientOption = options.Value;
    private HubConnection _hubConnection = null!;

    public async Task StartHubAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_webApiClientOption.HubAddress, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(sessionContext.AccessToken);
            })
            .WithAutomaticReconnect()
            .Build();
        _hubConnection.On<NotificationMessage>("Receive", notificationHubDispatcher.DispatchAsync);

        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to connect to the notification hub.");
        }
    }

    public ValueTask DisposeAsync()
    {
        return ((IAsyncDisposable)_hubConnection).DisposeAsync();
    }
}
