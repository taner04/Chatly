using System.Linq.Expressions;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Chatly.WebApi.Features.Users.Validation;

internal static class ProfilePictureValidationExtensions
{
    private const long MaximumFileSize = 5 * 1024 * 1024;

    private static readonly HashSet<string> SupportedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    internal static void AddProfilePictureRules<T>(
        this AbstractValidator<T> validator,
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
            .Must(value => value is not null && SupportedContentTypes.Contains(value))
            .WithMessage("Profile picture must be a JPEG, PNG, or WebP image.");

        validator.RuleFor(length)
            .GreaterThan(0)
            .WithMessage("Profile picture cannot be empty.")
            .LessThanOrEqualTo(MaximumFileSize)
            .WithMessage("Profile picture cannot exceed 5 MB.");
    }

    internal static void AddProfilePictureRules<T>(
        this AbstractValidator<T> validator,
        Expression<Func<T, IFormFile?>> file)
    {
        validator.RuleFor(file)
            .Must(value => value is { Length: > 0 })
            .WithMessage("Profile picture cannot be empty.")
            .Must(value => value is not null && value.Length <= MaximumFileSize)
            .WithMessage("Profile picture cannot exceed 5 MB.")
            .Must(value => value is not null && !string.IsNullOrWhiteSpace(value.FileName))
            .WithMessage("Profile picture file name cannot be empty.")
            .Must(value => value is not null && value.FileName.Length <= 255)
            .WithMessage("Profile picture file name cannot exceed 255 characters.")
            .Must(value => value is not null && SupportedContentTypes.Contains(value.ContentType))
            .WithMessage("Profile picture must be a JPEG, PNG, or WebP image.");
    }
}
