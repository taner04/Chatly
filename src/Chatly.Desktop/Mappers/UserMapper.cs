using Chatly.Contracts.Endpoints.Users.Results;

namespace Chatly.Desktop.Mappers;

public static class UserMapper
{
    public static User Map(CurrentUserResponse response)
    {
        return new User
        {
            Id = response.UserId,
            Email = response.Email,
            Username = response.Username,
            ProfilePictureUrl = response.ProfilePictureUrl,
            OnboardingCompleted = response.OnboardingCompleted
        };
    }
}
