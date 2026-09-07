using Chatly.WebApi.Features.Messages.Models;
using FluentValidation;

namespace Chatly.WebApi.Features.Messages.Endpoints.SendMessage;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.Content)
            .Must(content => !string.IsNullOrWhiteSpace(content))
            .WithMessage("Message content cannot be empty.")
            .MaximumLength(Message.MaxContentLength)
            .WithMessage($"Message content cannot exceed {Message.MaxContentLength} characters.");
    }
}