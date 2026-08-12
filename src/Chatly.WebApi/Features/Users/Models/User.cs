using Chatly.WebApi.Common.Helper;
using Chatly.WebApi.Common.Shared.Guards;
using Chatly.WebApi.Common.Shared.Models;
using Vogen;

namespace Chatly.WebApi.Features.Users.Models;

[ValueObject<Guid>]
public readonly partial struct UserId
{
    private static Validation Validate(Guid value) => value.Validate<UserId>();
}

public sealed class User : Entity<UserId>
{
    public const int MaxEmailLength = 320;
    public const int MaxAuth0IdLength = 256;
    public const int MaxUsernameLength = 32;
    public const int MaxProfilePictureKeyLength = 512;

    public User(string email, string auth0Id)
        : base(UserId.From(Guid.CreateVersion7()))
    {
        Guard.Against.InvalidEmail<User>(email);
        Guard.Against.NullOrEmpty<User>(auth0Id);

        Email = email;
        Auth0Id = auth0Id;
        Username = null;
        ProfilePictureKey = null;
        OnboardingCompleted = false;
    }

    public string Email { get; private set; }
    public string Auth0Id { get; private set; }
    public string? Username { get; private set; }
    public string? ProfilePictureKey { get; private set; }
    public bool OnboardingCompleted { get; private set; }
}