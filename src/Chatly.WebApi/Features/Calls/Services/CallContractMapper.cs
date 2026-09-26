using Chatly.Contracts.Features.Hubs;
using Chatly.WebApi.Features.Calls.Enums;
using ContractCallEndReason = Chatly.Contracts.Features.Hubs.CallEndReason;
using ContractCallState = Chatly.Contracts.Features.Hubs.CallState;
using DomainCall = Chatly.WebApi.Features.Calls.Models.Call;
using DomainCallEndReason = Chatly.WebApi.Features.Calls.Enums.CallEndReason;

namespace Chatly.WebApi.Features.Calls.Services;

internal static class CallContractMapper
{
    internal static CallInfo ToCallInfo(DomainCall call, UserId actorUserId)
    {
        var remoteUser = actorUserId == call.CallerUserId ? call.ReceiverUser : call.CallerUser;
        return new CallInfo(
            call.Id.Value,
            remoteUser.Id.Value,
            remoteUser.Username,
            actorUserId == call.CallerUserId ? CallRole.Caller : CallRole.Receiver,
            ToContract(call.Status),
            call.EndReason is null ? null : ToContract(call.EndReason.Value),
            call.AcceptedAt);
    }

    internal static ContractCallState ToContract(CallStatus status) => status switch
    {
        CallStatus.Ringing => ContractCallState.Ringing,
        CallStatus.Active => ContractCallState.Active,
        CallStatus.Ended => ContractCallState.Ended,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    internal static ContractCallEndReason ToContract(DomainCallEndReason reason) => reason switch
    {
        DomainCallEndReason.Completed => ContractCallEndReason.Completed,
        DomainCallEndReason.Declined => ContractCallEndReason.Declined,
        DomainCallEndReason.Cancelled => ContractCallEndReason.Cancelled,
        DomainCallEndReason.Missed => ContractCallEndReason.Missed,
        DomainCallEndReason.Failed => ContractCallEndReason.Failed,
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
    };
}
