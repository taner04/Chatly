using Chatly.Desktop.Abstraction.Hubs;

namespace Chatly.Desktop.Services.Api.Hubs;

internal abstract class HubMessageHandler<TMessage> : IHubMessageHandler
    where TMessage : IHubMessage
{
    public Type MessageType => typeof(TMessage);

    public Task HandleAsync(IHubMessage message) =>
        HandleMessageAsync((TMessage)message);

    protected abstract Task HandleMessageAsync(TMessage message);
}
