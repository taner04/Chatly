namespace Chatly.Desktop.Abstraction.Hubs;

public interface IHubMessageDispatcher
{
    Task DispatchAsync(IHubMessage message);
}