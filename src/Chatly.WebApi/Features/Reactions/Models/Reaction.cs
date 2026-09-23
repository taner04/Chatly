using Chatly.Contracts.Features.Reactions.Models;

namespace Chatly.WebApi.Features.Reactions.Models;

[ValueObject<Guid>]
public readonly partial struct ReactionId : IGuidEntityId<ReactionId>
{
    private static Validation Validate(Guid value) => value.Validate<ReactionId>();
}

public sealed class Reaction : Entity<ReactionId>
{
    private Reaction()
    {
    }

    public Reaction(UserId userId, MessageId messageId, ReactionType type)
    {
        UserId = userId;
        MessageId = messageId;
        Type = type;
    }

    public UserId UserId { get; private init; }

    public MessageId MessageId { get; private init; }

    public ReactionType Type { get; set; }
}
