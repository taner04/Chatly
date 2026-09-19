namespace Chatly.WebApi.Common.Extensions;

internal static class UserPairQueryExtensions
{
    extension<TEntity>(IQueryable<TEntity> query) where TEntity : class, IUserPairEntity
    {
        internal IQueryable<TEntity> ForUser(UserId userId)
        {
            return query.Where(pair =>
                EF.Property<UserId>(pair, nameof(IUserPairEntity.FirstUserId)) == userId ||
                EF.Property<UserId>(pair, nameof(IUserPairEntity.SecondUserId)) == userId);
        }

        internal IQueryable<UserId> SelectOtherUserId(UserId userId)
        {
            return query.Select(pair =>
                EF.Property<UserId>(pair, nameof(IUserPairEntity.FirstUserId)) == userId
                    ? EF.Property<UserId>(pair, nameof(IUserPairEntity.SecondUserId))
                    : EF.Property<UserId>(pair, nameof(IUserPairEntity.FirstUserId)));
        }
    }
}