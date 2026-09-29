using Chatly.Desktop.Abstraction.Hubs;

namespace Chatly.Desktop.Services.Api.Hubs;

[SingletonService(typeof(IHubMessageDispatcher))]
internal sealed partial class HubMessageDispatcher : IHubMessageDispatcher
{
    private readonly IReadOnlyDictionary<Type, IHubMessageHandler> _handlers;
    private readonly ILogger<HubMessageDispatcher> _logger;

    public HubMessageDispatcher(
        IEnumerable<IHubMessageHandler> handlers,
        ILogger<HubMessageDispatcher> logger)
    {
        _handlers = handlers.ToDictionary(handler => handler.MessageType);
        _logger = logger;
    }

    public async Task DispatchAsync(IHubMessage message)
    {
        var messageType = message.GetType();
        if (!_handlers.TryGetValue(messageType, out var handler))
        {
            LogNoHubMessageHandlerFoundForTypeMessagetype(messageType);
            return;
        }

        var handlerType = handler.GetType();
        LogDispatchingMessageOfTypeMessagetypeToHandlerHandlertype(messageType, handlerType);
        await handler.HandleAsync(message);
        LogFinishedDispatchingMessageOfTypeMessagetypeToHandlerHandlertype(messageType, handlerType);
    }

    [LoggerMessage(LogLevel.Warning, "No hub message handler found for type {MessageType}")]
    private partial void LogNoHubMessageHandlerFoundForTypeMessagetype(Type messageType);

    [LoggerMessage(LogLevel.Information, "Dispatching message of type {MessageType} to handler {HandlerType}")]
    private partial void LogDispatchingMessageOfTypeMessagetypeToHandlerHandlertype(Type messageType, Type handlerType);

    [LoggerMessage(LogLevel.Information, "Finished dispatching message of type {MessageType} to handler {HandlerType}")]
    private partial void LogFinishedDispatchingMessageOfTypeMessagetypeToHandlerHandlertype(Type messageType,
        Type handlerType);
}