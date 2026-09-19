namespace Chatly.WebApi.Features.Users.Endpoints.CompleteOnboarding;

internal sealed class CompleteOnboardingCommandValidator
    : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingCommandValidator()
    {
        RuleFor(command => command.NewUsername)
            .AddUsernameRules();

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