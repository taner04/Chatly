namespace Chatly.WebApi.Common.Shared.Models;

public abstract class UserPairEntity<TId> : Entity<TId>, IUserPairEntity
    where TId : struct, IGuidEntityId<TId>
{
    protected UserPairEntity()
    {
    }

    protected UserPairEntity(
        UserId firstUserId,
        UserId secondUserId)
    {
        var participants = UserPair.Create(firstUserId, secondUserId);

        FirstUserId = participants.FirstUserId;
        SecondUserId = participants.SecondUserId;
    }

    public UserId FirstUserId { get; }
    public UserId SecondUserId { get; }
}
