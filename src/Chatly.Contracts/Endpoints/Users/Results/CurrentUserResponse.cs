namespace Chatly.Contracts.Endpoints.Users.Results;

public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string? Username,
    string? ProfilePictureUrl,
    bool OnboardingCompleted);