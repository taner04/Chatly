using Chatly.WebApi.Features.Friendships.Models;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class FriendshipConfiguration
    : UserPairEntityConfiguration<Friendship, FriendshipId>
{
    protected override string TableName => "Friendships";
    protected override string DistinctUsersConstraintName => "CK_Friendships_DistinctUsers";
}