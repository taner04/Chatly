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
    internal const int MaxIdentityIdLength = 256;

    public User(string email, string identityId)
    {
        Guard.Against.InvalidEmail<User>(email);
        Guard.Against.NullOrEmpty<User>(identityId);

        Email = email;
        IdentityId = identityId;
        Username = null;
        ProfilePictureFileId = null;
        OnboardingCompleted = false;
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    public string Email { get; set; }
    public string IdentityId { get; private set; }
    public string? Username { get; set; }
    public StoredFileId? ProfilePictureFileId { get; set; }
    public StoredFile? ProfilePictureFile { get; set; }
    public bool OnboardingCompleted { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
    public DateTimeOffset? LastAbsenceEmailAt { get; set; }

    public ICollection<FriendRequest> SentFriendRequests { get; set; } = [];

    public ICollection<FriendRequest> ReceivedFriendRequests { get; set; } = [];
}