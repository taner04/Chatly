namespace Chatly.Desktop.Abstraction.Hubs;

public interface IHubMessageDispatcher<in TMessage> where TMessage : IHubMessage
{
    Task DispatchAsync(TMessage message);
}