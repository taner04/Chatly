using Chatly.WebApi.Features.FriendRequests.Enums;

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
        : base(FriendRequestId.From(Guid.CreateVersion7()), senderId, receiverId)
    {
        SenderUserId = senderId;
        ReceiverUserId = receiverId;
        Status = FriendRequestStatus.Pending;
    }

    public UserId SenderUserId { get; set; }

    public UserId ReceiverUserId { get; set; }

    public FriendRequestStatus Status { get; set; }

    public User SenderUser { get; init; } = null!;

    public User ReceiverUser { get; init; } = null!;
}