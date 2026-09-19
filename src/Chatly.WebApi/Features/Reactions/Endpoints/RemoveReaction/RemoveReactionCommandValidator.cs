namespace Chatly.WebApi.Features.Reactions.Endpoints.RemoveReaction;

internal sealed class RemoveReactionCommandValidator : AbstractValidator<RemoveReactionCommand>
{
    public RemoveReactionCommandValidator()
    {
        RuleFor(command => command.ReactionId)
            .NotEmptyVogenId(reactionId => reactionId.Value, "Reaction ID cannot be empty.");
    }
}