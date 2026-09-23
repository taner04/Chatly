namespace Chatly.Contracts.Features.Hubs.Abstraction;

public interface ICallingHubServer
{
    Task<CallInfo> StartCallAsync(Guid calleeUserId);

    Task<CallInfo?> GetCurrentCallAsync();

    Task AcceptCallAsync(Guid callId);

    Task RejectCallAsync(Guid callId);

    Task EndCallAsync(Guid callId);

    Task SendOfferAsync(Guid callId, string sdp);

    Task SendAnswerAsync(Guid callId, string sdp);

    Task SendIceCandidateAsync(Guid callId, string candidate, string? sdpMid, int? sdpMLineIndex);
}
