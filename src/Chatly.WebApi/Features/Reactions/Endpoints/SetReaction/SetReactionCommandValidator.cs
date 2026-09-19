namespace Chatly.WebApi.Features.Reactions.Endpoints.SetReaction;

internal sealed class SetReactionCommandValidator : AbstractValidator<SetReactionCommand>
{
    public SetReactionCommandValidator()
    {
        RuleFor(command => command.MessageId)
            .NotEmptyVogenId(messageId => messageId.Value, "Message ID cannot be empty.");

        RuleFor(command => command.ReactionType)
            .IsInEnum();
    }
}