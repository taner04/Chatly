namespace Chatly.Desktop.Abstraction.Hubs;

public interface IHubMessageHandler
{
    Type MessageType { get; }

    Task HandleAsync(IHubMessage message);
}