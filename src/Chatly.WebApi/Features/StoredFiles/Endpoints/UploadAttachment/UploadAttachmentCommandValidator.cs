namespace Chatly.WebApi.Features.StoredFiles.Endpoints.UploadAttachment;

internal sealed class UploadAttachmentCommandValidator : AbstractValidator<UploadAttachmentCommand>
{
    public UploadAttachmentCommandValidator()
    {
        RuleFor(command => command.File)
            .NotNull()
            .WithMessage("An attachment file is required.");

        When(command => command.File is not null, () =>
        {
            RuleFor(command => command.File!)
                .AddAttachmentFileRules();
        });
    }
}