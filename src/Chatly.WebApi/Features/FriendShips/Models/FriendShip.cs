using Chatly.WebApi.Common.Helper;
using Chatly.WebApi.Common.Shared.Models;
using Chatly.WebApi.Features.Users.Models;
using Vogen;

namespace Chatly.WebApi.Features.FriendShips.Models;

[ValueObject<Guid>]
public readonly partial struct FriendShipId
{
    private static Validation Validate(Guid value) => value.Validate<FriendShipId>();
}

public sealed class FriendShip : UserPairEntity<FriendShipId>
{
    public FriendShip(
        UserId firstUserId,
        UserId secondUserId)
        : base(FriendShipId.From(Guid.CreateVersion7()), firstUserId, secondUserId)
    {
    }
}