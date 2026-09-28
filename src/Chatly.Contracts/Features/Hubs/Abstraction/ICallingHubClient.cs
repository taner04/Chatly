namespace Chatly.Contracts.Features.Hubs.Abstraction;

public interface ICallingHubClient : IHubClient
{
    Task Receive(CallMessage call);
}