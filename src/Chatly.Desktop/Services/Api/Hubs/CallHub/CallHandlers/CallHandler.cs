using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Calls;

namespace Chatly.Desktop.Services.Api.Hubs.CallHub.CallHandlers;

internal abstract partial class CallHandler<TCall>(
    CallCoordinator coordinator,
    ILogger<CallHandler<TCall>> logger) : IHubMessageHandler<Call>
    where TCall : Call
{
    public Type MessageType => typeof(TCall);

    public async Task HandleAsync(Call message)
    {
        if (message is not TCall typedMessage)
        {
            LogUnexpectedMessageType(message.GetType().Name, typeof(TCall).Name);
            return;
        }

        await coordinator.HandleAsync(typedMessage);
        LogMessageHandled(typeof(TCall).Name, typedMessage.CallId);
    }

    [LoggerMessage(LogLevel.Warning, "Received call message of type {MessageType} but expected {ExpectedType}")]
    private partial void LogUnexpectedMessageType(string messageType, string expectedType);

    [LoggerMessage(LogLevel.Information, "Handled {MessageType} for call {CallId}")]
    private partial void LogMessageHandled(string messageType, Guid callId);
}
