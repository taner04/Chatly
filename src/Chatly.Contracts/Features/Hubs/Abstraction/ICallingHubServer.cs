namespace Chatly.Contracts.Features.Hubs.Abstraction;

public interface ICallingHubServer
{
    Task<CallInfo> StartCallAsync(Guid calleeUserId);

    Task<CallInfo?> GetCurrentCallAsync();

    Task<CallInfo> AcceptCallAsync(Guid callId);

    Task RejectCallAsync(Guid callId);

    Task EndCallAsync(Guid callId);

    Task<CallMediaAccess> JoinMediaAsync(Guid callId);
}