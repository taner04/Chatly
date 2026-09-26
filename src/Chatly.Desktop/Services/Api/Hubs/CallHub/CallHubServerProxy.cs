namespace Chatly.Desktop.Services.Api.Hubs.CallHub;

[SingletonService(typeof(ICallingHubServer))]
internal sealed class CallHubServerProxy(CallHubConnection connection) : ICallingHubServer
{
    public Task<CallInfo> StartCallAsync(Guid calleeUserId) =>
        connection.InvokeAsync<CallInfo>(nameof(ICallingHubServer.StartCallAsync), calleeUserId);

    public Task<CallMediaAccess> JoinMediaAsync(Guid callId) =>
        connection.InvokeAsync<CallMediaAccess>(nameof(ICallingHubServer.JoinMediaAsync), callId);

    public Task<CallInfo?> GetCurrentCallAsync() =>
        connection.InvokeAsync<CallInfo?>(nameof(ICallingHubServer.GetCurrentCallAsync));

    public Task<CallInfo> AcceptCallAsync(Guid callId) =>
        connection.InvokeAsync<CallInfo>(nameof(ICallingHubServer.AcceptCallAsync), callId);

    public Task RejectCallAsync(Guid callId) =>
        connection.InvokeAsync(nameof(ICallingHubServer.RejectCallAsync), callId);

    public Task EndCallAsync(Guid callId) =>
        connection.InvokeAsync(nameof(ICallingHubServer.EndCallAsync), callId);
}