using Chatly.WebApi.Common.Helper;
using Chatly.WebApi.Common.Shared.Guards;
using Chatly.WebApi.Common.Shared.Models;
using System.ComponentModel.DataAnnotations.Schema;
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

    public string Email { get; set; }
    public string Auth0Id { get; private set; }
    public string? Username { get; set; }
    public string? ProfilePictureKey { get; set; }
    public bool OnboardingCompleted { get; set; }

}
