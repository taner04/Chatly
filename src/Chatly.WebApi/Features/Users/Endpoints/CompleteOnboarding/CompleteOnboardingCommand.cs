namespace Chatly.WebApi.Features.Users.Endpoints.CompleteOnboarding;

internal sealed record CompleteOnboardingCommand(
    string NewUsername,
    Stream? Content,
    string? FileName,
    string? ContentType,
    long Length) : ICommand<CurrentUserResponse>;
