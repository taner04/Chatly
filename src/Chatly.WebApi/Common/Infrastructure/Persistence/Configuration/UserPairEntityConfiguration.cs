using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal abstract class UserPairEntityConfiguration<TEntity, TId>
    : EntityConfiguration<TEntity, TId>
    where TEntity : UserPairEntity<TId>
    where TId : struct
{
    protected abstract string TableName { get; }
    protected abstract string DistinctUsersConstraintName { get; }
    protected virtual bool ConfigureParticipantForeignKeys => true;

    protected sealed override void PostConfigure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(pair => pair.FirstUserId)
            .IsRequired();

        builder.Property(pair => pair.SecondUserId)
            .IsRequired();

        if (ConfigureParticipantForeignKeys)
        {
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(pair => pair.FirstUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(pair => pair.SecondUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        builder.HasIndex(pair => new
            {
                pair.FirstUserId,
                pair.SecondUserId
            })
            .IsUnique();

        builder.ToTable(TableName, table => table.HasCheckConstraint(
            DistinctUsersConstraintName,
            "\"FirstUserId\" <> \"SecondUserId\""));

        ConfigureUserPair(builder);
    }

    protected virtual void ConfigureUserPair(EntityTypeBuilder<TEntity> builder)
    {
    }
}