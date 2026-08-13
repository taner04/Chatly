namespace Chatly.Contracts.Users.Results;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string? Username,
    string? ProfilePictureUrl,
    bool OnboardingCompleted);