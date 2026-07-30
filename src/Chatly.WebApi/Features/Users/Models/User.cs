using Chatly.WebApi.Common.Shared.Guards;
using Chatly.WebApi.Common.Shared.Models;
using Chatly.WebApi.Features.Chats.Models;
using Vogen;

namespace Chatly.WebApi.Features.Users.Models;

[ValueObject<Guid>]
public readonly partial struct UserId
{
    private static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("UserId must be set to a non-default value.");
}

public sealed class User : Entity<UserId>
{
    private User()
    {
    }

    public User(string email, string auth0Id)
    {
        Guard.Against.InvalidEmail<User>(email);
        Guard.Against.NullOrEmpty<User>(auth0Id);

        Id = UserId.From(Guid.CreateVersion7());
        Email = email;
        Auth0Id = auth0Id;
    }

    public string Email { get; private set; } = null!;
    public string Auth0Id { get; private set; } = null!;

    public ICollection<ChatMember> ChatMemberships { get; } = [];
    public ICollection<Chat> Chats { get; } = [];
}
