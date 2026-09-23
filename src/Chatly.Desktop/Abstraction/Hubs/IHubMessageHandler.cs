namespace Chatly.Desktop.Abstraction.Hubs;

internal interface IHubMessageHandler<in TMessage> where TMessage : IHubMessage
{
    Type MessageType { get; }

    Task HandleAsync(TMessage message);
}