namespace Chatly.Contracts.Features.Users.Endpoints.GetCurrentUser;

public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string? Username,
    string? ProfilePictureUrl,
    bool OnboardingCompleted);