using Chatly.WebApi.Common.Helper;
using Chatly.WebApi.Common.Shared.Models;
using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.Users.Models;
using Vogen;

namespace Chatly.WebApi.Features.FriendRequests.Models;


[ValueObject<Guid>]
public readonly partial struct FriendRequestId
{
    private static Validation Validate(Guid value) => value.Validate<FriendRequestId>();
}

public sealed class FriendRequest : UserPairEntity<FriendRequestId>
{
    private FriendRequest()
    {
    }

    public FriendRequest(UserId senderId, UserId receiverId)
        : base(
            FriendRequestId.From(Guid.CreateVersion7()),
            senderId,
            receiverId)
    {
        RequestedByUserId = senderId;
        Status = FriendRequestStatus.Pending;
    }

    public UserId RequestedByUserId { get; set; }

    public FriendRequestStatus Status { get; set; }
}
