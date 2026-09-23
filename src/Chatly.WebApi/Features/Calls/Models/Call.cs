using Chatly.WebApi.Features.Calls.Enums;

namespace Chatly.WebApi.Features.Calls.Models;

[ValueObject<Guid>]
public readonly partial struct CallId : IGuidEntityId<CallId>
{
    private static Validation Validate(Guid value) => value.Validate<CallId>();
}

public sealed class Call : Entity<CallId>
{
    private Call()
    {
    }

    public Call(UserId callerUserId, UserId receiverUserId)
    {
        if (callerUserId == receiverUserId)
        {
            throw new ArgumentException("A user cannot call themselves.");
        }

        CallerUserId = callerUserId;
        ReceiverUserId = receiverUserId;
        Status = CallStatus.Ringing;
        InitiatedAt = DateTimeOffset.UtcNow;
    }

    public UserId CallerUserId { get; private init; }

    public UserId ReceiverUserId { get; private init; }

    public CallStatus Status { get; private set; }

    public CallEndReason? EndReason { get; private set; }

    public DateTimeOffset InitiatedAt { get; private init; }

    public DateTimeOffset? AcceptedAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public User CallerUser { get; private init; } = null!;

    public User ReceiverUser { get; private init; } = null!;

    public ICollection<ActiveCallParticipant> ActiveParticipants { get; private init; } = [];

    public UserId GetCounterpart(UserId actorUserId)
    {
        EnsureParticipant(actorUserId);
        return actorUserId == CallerUserId ? ReceiverUserId : CallerUserId;
    }

    public void Accept(UserId actorUserId, DateTimeOffset timestamp)
    {
        EnsureRole(actorUserId, ReceiverUserId, "Only the receiver can accept the call.");
        EnsureStatus(CallStatus.Ringing);
        Status = CallStatus.Accepted;
        AcceptedAt = timestamp;
    }

    public void Reject(UserId actorUserId, DateTimeOffset timestamp)
    {
        EnsureRole(actorUserId, ReceiverUserId, "Only the receiver can reject the call.");
        EnsureStatus(CallStatus.Ringing);
        Finish(CallEndReason.Declined, timestamp);
    }

    public void Offer(UserId actorUserId)
    {
        EnsureRole(actorUserId, CallerUserId, "Only the caller can send the offer.");
        EnsureStatus(CallStatus.Accepted);
        Status = CallStatus.Offered;
    }

    public void Answer(UserId actorUserId)
    {
        EnsureRole(actorUserId, ReceiverUserId, "Only the receiver can send the answer.");
        EnsureStatus(CallStatus.Offered);
        Status = CallStatus.Active;
    }

    public void EnsureCanSendIce(UserId actorUserId)
    {
        EnsureParticipant(actorUserId);
        if (Status is not (CallStatus.Offered or CallStatus.Active))
        {
            throw new CallTransitionException("ICE candidates require an offered or active call.");
        }
    }

    public void End(UserId actorUserId, DateTimeOffset timestamp)
    {
        EnsureParticipant(actorUserId);
        if (Status == CallStatus.Ended)
        {
            throw new CallTransitionException("The call has already ended.");
        }

        var reason = Status switch
        {
            CallStatus.Ringing when actorUserId == CallerUserId => CallEndReason.Cancelled,
            CallStatus.Ringing => CallEndReason.Declined,
            CallStatus.Accepted or CallStatus.Offered => CallEndReason.Failed,
            CallStatus.Active => CallEndReason.Completed,
            _ => throw new CallTransitionException("The call cannot be ended from its current state.")
        };
        Finish(reason, timestamp);
    }

    public void Expire(DateTimeOffset timestamp)
    {
        EnsureStatus(CallStatus.Ringing);
        Finish(CallEndReason.Missed, timestamp);
    }

    public void ExpireAbandoned(DateTimeOffset timestamp)
    {
        if (Status is CallStatus.Ringing or CallStatus.Ended)
        {
            throw new CallTransitionException("Only abandoned non-ringing calls can be expired.");
        }

        Finish(CallEndReason.Failed, timestamp);
    }

    private void Finish(CallEndReason reason, DateTimeOffset timestamp)
    {
        Status = CallStatus.Ended;
        EndReason = reason;
        EndedAt = timestamp;
    }

    private void EnsureParticipant(UserId actorUserId)
    {
        if (actorUserId != CallerUserId && actorUserId != ReceiverUserId)
        {
            throw new UnauthorizedAccessException("The user is not a participant in this call.");
        }
    }

    private static void EnsureRole(UserId actorUserId, UserId expectedUserId, string message)
    {
        if (actorUserId != expectedUserId)
        {
            throw new UnauthorizedAccessException(message);
        }
    }

    private void EnsureStatus(CallStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new CallTransitionException($"The call must be {expectedStatus}.");
        }
    }
}
