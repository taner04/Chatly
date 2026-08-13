using Chatly.WebApi.Features.Users.Common;
using FluentValidation;

namespace Chatly.WebApi.Features.Users.Endpoints.CompleteOnboarding;

public sealed class CompleteOnboardingCommandValidator
    : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingCommandValidator()
    {
        RuleFor(command => command.NewUsername)
            .NotEmpty()
            .WithMessage("Username cannot be empty.")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Username can only contain letters, numbers, and underscores.");

        When(command => command.Content is not null, () =>
        {
            this.AddProfilePictureRules(
                command => command.Content,
                command => command.FileName,
                command => command.ContentType,
                command => command.Length);
        });
    }
}
