namespace Chatly.WebApi.Features.StoredFiles.Endpoints.RemoveAttachment;

internal sealed class RemoveAttachmentCommandValidator : AbstractValidator<RemoveAttachmentCommand>
{
    public RemoveAttachmentCommandValidator()
    {
        RuleFor(command => command.StoredFileId)
            .NotEmptyVogenId(id => id.Value, "Stored file ID cannot be empty.");
    }
}