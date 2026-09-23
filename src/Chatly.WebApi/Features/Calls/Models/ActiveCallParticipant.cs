namespace Chatly.WebApi.Features.Calls.Models;

[ValueObject<Guid>]
public readonly partial struct ActiveCallParticipantId : IGuidEntityId<ActiveCallParticipantId>
{
    private static Validation Validate(Guid value) => value.Validate<ActiveCallParticipantId>();
}

public sealed class ActiveCallParticipant : Entity<ActiveCallParticipantId>
{
    private ActiveCallParticipant()
    {
    }

    public ActiveCallParticipant(UserId userId, CallId callId)
    {
        UserId = userId;
        CallId = callId;
    }

    public UserId UserId { get; private init; }

    public CallId CallId { get; private init; }

    public User User { get; private init; } = null!;

    public Call Call { get; private init; } = null!;
}
