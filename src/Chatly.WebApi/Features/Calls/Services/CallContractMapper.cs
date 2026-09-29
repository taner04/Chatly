using Chatly.Contracts.Features.Hubs;
using Chatly.WebApi.Features.Calls.Models;

namespace Chatly.WebApi.Features.Calls.Services;

internal static class CallContractMapper
{
    internal static CallInfo ToCallInfo(Call call, UserId actorUserId)
    {
        var remoteUser = actorUserId == call.CallerUserId ? call.ReceiverUser : call.CallerUser;
        return new CallInfo(
            call.Id.Value,
            remoteUser.Id.Value,
            remoteUser.Username,
            actorUserId == call.CallerUserId ? CallRole.Caller : CallRole.Receiver,
            call.Status,
            call.EndReason,
            call.AcceptedAt);
    }
}