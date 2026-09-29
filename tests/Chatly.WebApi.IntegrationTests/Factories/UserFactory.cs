namespace Chatly.WebApi.IntegrationTests.Factories;

public sealed record TestUser(UserId Id, string Sub, string Email, string Username);

internal static class UserFactory
{
    internal const string DefaultUsername = "current_user";

    internal static User Create(string username)
    {
        var user = new User($"{username}@chatly.tests", $"auth0|{username}")
        {
            Username = username,
            OnboardingCompleted = true
        };
        user.SetCreated("tests");
        return user;
    }

    internal static TestUser ToTestUser(User user) => new(user.Id, user.Auth0Id, user.Email, user.Username!);
}