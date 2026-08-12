namespace Chatly.WebApi.Features.Users.Models;

public readonly record struct UserPair
{
    private UserPair(UserId firstUserId, UserId secondUserId)
    {
        FirstUserId = firstUserId;
        SecondUserId = secondUserId;
    }

    public static UserPair Create(UserId firstUserId, UserId secondUserId)
    {
        if (firstUserId == secondUserId)
        {
            throw new ArgumentException("A user pair must contain two different users.");
        }

        return firstUserId.Value.CompareTo(secondUserId.Value) < 0
            ? new UserPair(firstUserId, secondUserId)
            : new UserPair(secondUserId, firstUserId);
    }

#pragma warning disable VOG038
    public UserId FirstUserId { get; init; }
    public UserId SecondUserId { get; init; }
#pragma warning restore VOG038
}