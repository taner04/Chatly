using Chatly.Contracts.Features.Hubs;

namespace Chatly.WebApi.Features.Calls.Models;

[ValueObject<Guid>]
public readonly partial struct CallId : IGuidEntityId<CallId>
{
    private static Validation Validate(Guid value) => value.Validate<CallId>();
}

public sealed class Call : Entity<CallId>
{
    [UsedImplicitly]
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
        Status = CallState.Ringing;
        InitiatedAt = DateTimeOffset.UtcNow;
    }

    public UserId CallerUserId { get; }

    public UserId ReceiverUserId { get; }

    public CallState Status { get; internal set; }

    public CallEndReason? EndReason { get; internal set; }

    public DateTimeOffset InitiatedAt { get; private init; }

    public DateTimeOffset? AcceptedAt { get; internal set; }

    public DateTimeOffset? EndedAt { get; internal set; }

    public User CallerUser { get; private init; } = null!;

    public User ReceiverUser { get; private init; } = null!;

    public ICollection<ActiveCallParticipant> ActiveParticipants { get; private init; } = [];
}