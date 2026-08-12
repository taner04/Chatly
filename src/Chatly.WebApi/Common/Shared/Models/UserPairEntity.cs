using System.ComponentModel.DataAnnotations.Schema;
using Chatly.WebApi.Features.Users.Models;

namespace Chatly.WebApi.Common.Shared.Models;

public abstract class UserPairEntity<TId> : Entity<TId>
    where TId : struct
{
    protected UserPairEntity()
    {
    }

    protected UserPairEntity(
        TId id,
        UserId firstUserId,
        UserId secondUserId)
        : base(id)
    {
        var participants = UserPair.Create(firstUserId, secondUserId);

        FirstUserId = participants.FirstUserId;
        SecondUserId = participants.SecondUserId;
    }

    public UserId FirstUserId { get; }
    public UserId SecondUserId { get; }

    [NotMapped] public UserPair Participants => UserPair.Create(FirstUserId, SecondUserId);
}