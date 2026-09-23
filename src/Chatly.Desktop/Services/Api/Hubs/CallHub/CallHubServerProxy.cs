namespace Chatly.Desktop.Services.Api.Hubs.CallHub;

[SingletonService(typeof(ICallingHubServer))]
internal sealed class CallHubServerProxy(CallHubConnection connection) : ICallingHubServer
{
    public Task<CallInfo> StartCallAsync(Guid calleeUserId) =>
        connection.InvokeAsync<CallInfo>(nameof(ICallingHubServer.StartCallAsync), calleeUserId);

    public Task<CallInfo?> GetCurrentCallAsync() =>
        connection.InvokeAsync<CallInfo?>(nameof(ICallingHubServer.GetCurrentCallAsync));

    public Task AcceptCallAsync(Guid callId) =>
        connection.InvokeAsync(nameof(ICallingHubServer.AcceptCallAsync), callId);

    public Task RejectCallAsync(Guid callId) =>
        connection.InvokeAsync(nameof(ICallingHubServer.RejectCallAsync), callId);

    public Task EndCallAsync(Guid callId) =>
        connection.InvokeAsync(nameof(ICallingHubServer.EndCallAsync), callId);

    public Task SendOfferAsync(Guid callId, string sdp) =>
        connection.InvokeAsync(nameof(ICallingHubServer.SendOfferAsync), callId, sdp);

    public Task SendAnswerAsync(Guid callId, string sdp) =>
        connection.InvokeAsync(nameof(ICallingHubServer.SendAnswerAsync), callId, sdp);

    public Task SendIceCandidateAsync(
        Guid callId,
        string candidate,
        string? sdpMid,
        int? sdpMLineIndex) =>
        connection.InvokeAsync(
            nameof(ICallingHubServer.SendIceCandidateAsync),
            callId,
            candidate,
            sdpMid,
            sdpMLineIndex);
}
