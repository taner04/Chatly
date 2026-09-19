namespace Chatly.WebApi.Features.Messages.Endpoints.RemoveMessage;

internal sealed class RemoveMessageCommandValidator : AbstractValidator<RemoveMessageCommand>
{
    public RemoveMessageCommandValidator()
    {
        RuleFor(command => command.MessageId)
            .NotEmptyVogenId(messageId => messageId.Value, "Message ID cannot be empty.");
    }
}