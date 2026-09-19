using Chatly.Contracts.Features.Users.Endpoints.GetCurrentUser;

namespace Chatly.Desktop.Mappers;

internal static class UserMapper
{
    public static User Map(CurrentUserResponse response) =>
        new()
        {
            Id = response.UserId,
            Email = response.Email,
            Username = response.Username,
            ProfilePictureUrl = response.ProfilePictureUrl,
            OnboardingCompleted = response.OnboardingCompleted
        };
}