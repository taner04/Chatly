using Chatly.WebApi.Common.Shared.Guards;
using Chatly.WebApi.Features.FriendRequests.Models;
using Chatly.WebApi.Features.StoredFiles.Models;

namespace Chatly.WebApi.Features.Users.Models;

[ValueObject<Guid>]
public readonly partial struct UserId : IGuidEntityId<UserId>
{
    private static Validation Validate(Guid value) => value.Validate<UserId>();
}

public sealed class User : Entity<UserId>
{
    internal const int MaxEmailLength = 320;
    internal const int MaxAuth0IdLength = 256;

    public User(string email, string auth0Id)
    {
        Guard.Against.InvalidEmail<User>(email);
        Guard.Against.NullOrEmpty<User>(auth0Id);

        Email = email;
        Auth0Id = auth0Id;
        Username = null;
        ProfilePictureFileId = null;
        OnboardingCompleted = false;
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    public string Email { get; set; }
    public string Auth0Id { get; private set; }
    public string? Username { get; set; }
    public StoredFileId? ProfilePictureFileId { get; set; }
    public StoredFile? ProfilePictureFile { get; set; }
    public bool OnboardingCompleted { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
    public DateTimeOffset? LastAbsenceEmailAt { get; set; }

    public ICollection<FriendRequest> SentFriendRequests { get; set; } = [];

    public ICollection<FriendRequest> ReceivedFriendRequests { get; set; } = [];
}
