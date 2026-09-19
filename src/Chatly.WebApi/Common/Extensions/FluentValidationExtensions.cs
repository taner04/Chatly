using System.Linq.Expressions;
using Chatly.Contracts.Common.Policies;
using Chatly.WebApi.Features.StoredFiles.Models;

namespace Chatly.WebApi.Common.Extensions;

internal static class FluentValidationExtensions
{
    private const long MaximumProfilePictureFileSize = 5 * 1024 * 1024;

    extension<T, TId>(IRuleBuilder<T, TId> ruleBuilder) where TId : struct
    {
        internal IRuleBuilderOptions<T, TId> NotEmptyVogenId(
            Func<TId, Guid> valueSelector,
            string errorMessage)
        {
            return ruleBuilder
                .Must(id => valueSelector(id) != Guid.Empty)
                .WithMessage(errorMessage);
        }
    }

    extension<T>(IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        internal IRuleBuilderOptions<T, IFormFile> AddAttachmentFileRules()
        {
            return ruleBuilder
                .Must(file => file.Length > 0)
                .WithMessage("Attachment file cannot be empty.")
                .Must(file => file.Length <= MessageAttachmentPolicy.MaxFileSizeBytes)
                .WithMessage(
                    $"Attachment file cannot exceed {MessageAttachmentPolicy.MaxFileSizeBytes} bytes.")
                .Must(file => !string.IsNullOrWhiteSpace(file.FileName))
                .WithMessage("Attachment file name is required.")
                .Must(file => file.FileName.Length <= StoredFile.MaxFileNameLength)
                .WithMessage(
                    $"Attachment file name cannot exceed {StoredFile.MaxFileNameLength} characters.")
                .Must(file => !string.IsNullOrWhiteSpace(file.ContentType))
                .WithMessage("Attachment content type is required.")
                .Must(file => file.ContentType.Length <= StoredFile.MaxContentTypeLength)
                .WithMessage(
                    $"Attachment content type cannot exceed {StoredFile.MaxContentTypeLength} characters.");
        }
    }

    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        internal IRuleBuilderOptions<T, string> AddUsernameRules() =>
            ruleBuilder
                .NotEmpty()
                .WithMessage("Username cannot be empty.")
                .MaximumLength(UsernamePolicy.MaxLength)
                .WithMessage($"Username cannot exceed {UsernamePolicy.MaxLength} characters.")
                .Matches(UsernamePolicy.Pattern)
                .WithMessage("Username can only contain letters, numbers, and underscores.");
    }

    extension<T>(AbstractValidator<T> validator)
    {
        internal void AddProfilePictureRules(
            Expression<Func<T, Stream?>> content,
            Expression<Func<T, string?>> fileName,
            Expression<Func<T, string?>> contentType,
            Expression<Func<T, long>> length)
        {
            validator.RuleFor(content)
                .NotNull()
                .Must(stream => stream is { CanRead: true })
                .WithMessage("Profile picture content must be readable.");

            validator.RuleFor(fileName)
                .NotEmpty()
                .MaximumLength(255);

            validator.RuleFor(contentType)
                .NotEmpty()
                .Must(value =>
                    value is not null &&
                    ImageContentTypePolicy.ProfilePictureContentTypes.Contains(value))
                .WithMessage("Profile picture must be a JPEG, PNG, or WebP image.");

            validator.RuleFor(length)
                .GreaterThan(0)
                .WithMessage("Profile picture cannot be empty.")
                .LessThanOrEqualTo(MaximumProfilePictureFileSize)
                .WithMessage("Profile picture cannot exceed 5 MB.");
        }

        internal void AddProfilePictureRules(Expression<Func<T, IFormFile?>> file)
        {
            validator.RuleFor(file)
                .Must(value => value is { Length: > 0 })
                .WithMessage("Profile picture cannot be empty.")
                .Must(value => value is not null && value.Length <= MaximumProfilePictureFileSize)
                .WithMessage("Profile picture cannot exceed 5 MB.")
                .Must(value => value is not null && !string.IsNullOrWhiteSpace(value.FileName))
                .WithMessage("Profile picture file name cannot be empty.")
                .Must(value => value is not null && value.FileName.Length <= 255)
                .WithMessage("Profile picture file name cannot exceed 255 characters.")
                .Must(value =>
                    value is not null &&
                    ImageContentTypePolicy.ProfilePictureContentTypes.Contains(value.ContentType))
                .WithMessage("Profile picture must be a JPEG, PNG, or WebP image.");
        }
    }
}