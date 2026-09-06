using Chatly.WebApi.Features.Friendships.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class FriendshipConfiguration
    : EntityConfiguration<Friendship, FriendshipId>
{
    protected override void PostConfigure(EntityTypeBuilder<Friendship> builder)
    {
        builder.Property(friendship => friendship.FirstUserId)
            .IsRequired();

        builder.Property(friendship => friendship.SecondUserId)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(friendship => friendship.FirstUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(friendship => friendship.SecondUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(friendship => new
            {
                friendship.FirstUserId,
                friendship.SecondUserId
            })
            .IsUnique();

        builder.ToTable("Friendships", table =>
        {
            table.HasCheckConstraint(
                "CK_Friendships_DistinctUsers",
                "\"FirstUserId\" <> \"SecondUserId\"");
        });
    }
}