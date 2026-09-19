using Chatly.Contracts.Common.Policies;

namespace Chatly.WebApi.Features.Messages.Endpoints.SendMessage;

internal sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(command => command.Content)
            .MaximumLength(Message.MaxContentLength)
            .WithMessage($"Message content cannot exceed {Message.MaxContentLength} characters.");

        RuleFor(command => command)
            .Must(command => !string.IsNullOrWhiteSpace(command.Content) || command.Files.Count > 0)
            .WithMessage("A message must contain text or at least one file.");

        RuleFor(command => command.Files)
            .Must(files => files.Count <= MessageAttachmentPolicy.MaxPerMessage)
            .WithMessage($"A message cannot contain more than {MessageAttachmentPolicy.MaxPerMessage} files.")
            .Must(files => files.Sum(file => file.Length) <=
                           MessageAttachmentPolicy.MaxTotalSizePerMessageBytes)
            .WithMessage(
                $"Message attachments cannot exceed {MessageAttachmentPolicy.MaxTotalSizePerMessageBytes} bytes in total.");

        RuleForEach(command => command.Files)
            .AddAttachmentFileRules();
    }
}