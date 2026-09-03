namespace Chatly.WebApi.Features.Users.Endpoints.CompleteOnboarding;

public sealed record CompleteOnboardingCommand(
    string NewUsername,
    Stream? Content,
    string? FileName,
    string? ContentType,
    long Length) : ICommand<CurrentUserResponse>;