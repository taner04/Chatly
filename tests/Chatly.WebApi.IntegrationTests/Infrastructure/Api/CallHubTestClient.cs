using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Abstraction;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Api;

public sealed class CallHubTestClient(HubConnection connection) : HubTestClient<CallMessage>(connection)
{
    public Task<CallInfo> StartCallAsync(Guid calleeUserId) =>
        Connection.InvokeAsync<CallInfo>(nameof(ICallingHubServer.StartCallAsync), calleeUserId);

    public Task<CallInfo?> GetCurrentCallAsync() =>
        Connection.InvokeAsync<CallInfo?>(nameof(ICallingHubServer.GetCurrentCallAsync));

    public Task<CallInfo> AcceptCallAsync(Guid callId) =>
        Connection.InvokeAsync<CallInfo>(nameof(ICallingHubServer.AcceptCallAsync), callId);

    public Task RejectCallAsync(Guid callId) =>
        Connection.InvokeAsync(nameof(ICallingHubServer.RejectCallAsync), callId);

    public Task EndCallAsync(Guid callId) =>
        Connection.InvokeAsync(nameof(ICallingHubServer.EndCallAsync), callId);

    public Task<CallMediaAccess> JoinMediaAsync(Guid callId) =>
        Connection.InvokeAsync<CallMediaAccess>(nameof(ICallingHubServer.JoinMediaAsync), callId);
}