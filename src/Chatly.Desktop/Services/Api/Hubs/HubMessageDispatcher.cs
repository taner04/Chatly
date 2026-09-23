using Chatly.Desktop.Abstraction.Hubs;

namespace Chatly.Desktop.Services.Api.Hubs;

[SingletonService(typeof(IHubMessageDispatcher<>))]
internal sealed class HubMessageDispatcher<TMessage> : IHubMessageDispatcher<TMessage>
    where TMessage : IHubMessage
{
    private readonly IReadOnlyDictionary<Type, IHubMessageHandler<TMessage>> _handlers;
    private readonly ILogger<HubMessageDispatcher<TMessage>> _logger;

    public HubMessageDispatcher(
        IEnumerable<IHubMessageHandler<TMessage>> handlers,
        ILogger<HubMessageDispatcher<TMessage>> logger)
    {
        _handlers = handlers.ToDictionary(handler => handler.MessageType);
        _logger = logger;
    }

    public async Task DispatchAsync(TMessage message)
    {
        var messageType = message.GetType();
        if (!_handlers.TryGetValue(messageType, out var handler))
        {
            _logger.LogWarning("No hub message handler found for type {MessageType}", messageType);
            return;
        }

        await handler.HandleAsync(message);
    }
}