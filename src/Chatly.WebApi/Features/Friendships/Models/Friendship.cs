namespace Chatly.WebApi.Features.Friendships.Models;

[ValueObject<Guid>]
public readonly partial struct FriendshipId
{
    private static Validation Validate(Guid value) => value.Validate<FriendshipId>();
}

public sealed class Friendship : UserPairEntity<FriendshipId>
{
    private Friendship()
    {
    }

    public Friendship(UserId firstUserId, UserId secondUserId)
        : base(FriendshipId.From(Guid.CreateVersion7()), firstUserId, secondUserId)
    {
    }
}