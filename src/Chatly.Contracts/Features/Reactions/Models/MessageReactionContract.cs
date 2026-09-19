namespace Chatly.Contracts.Features.Reactions.Models;

public sealed record MessageReactionContract(
    Guid ReactionId,
    Guid UserId,
    ReactionType ReactionType);