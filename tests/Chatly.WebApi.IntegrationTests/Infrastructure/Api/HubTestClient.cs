using System.Threading.Channels;
using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Abstraction;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Api;

public abstract class HubTestClient<TMessage> : IAsyncDisposable where TMessage : class, IHubMessage
{
    private readonly Channel<TMessage> _received = Channel.CreateUnbounded<TMessage>();

    protected HubTestClient(HubConnection connection)
    {
        Connection = connection;
        Connection.On<TMessage>(nameof(INotificationHubClient.Receive), message => _received.Writer.TryWrite(message));
    }

    private static TimeSpan ReceiveTimeout => TimeSpan.FromSeconds(10);

    public HubConnection Connection { get; }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public async Task<T> ReceiveAsync<T>(Func<T, bool>? predicate = null) where T : TMessage
    {
        using var timeout = new CancellationTokenSource(ReceiveTimeout);
        while (true)
        {
            var message = await _received.Reader.ReadAsync(timeout.Token);
            if (message is T expected && (predicate is null || predicate(expected)))
            {
                return expected;
            }
        }
    }

    public IReadOnlyList<T> ReceivedSoFar<T>() where T : TMessage
    {
        var messages = new List<T>();
        while (_received.Reader.TryRead(out var message))
        {
            if (message is T expected)
            {
                messages.Add(expected);
            }
        }

        return messages;
    }
}