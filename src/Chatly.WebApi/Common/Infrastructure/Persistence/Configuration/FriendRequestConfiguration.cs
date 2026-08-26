using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.FriendRequests.Models;
using Chatly.WebApi.Features.Users.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class FriendRequestConfiguration
    : EntityConfiguration<FriendRequest, FriendRequestId>
{
    protected override void PostConfigure(
        EntityTypeBuilder<FriendRequest> builder)
    {
        builder.Property(request => request.FirstUserId)
            .IsRequired();

        builder.Property(request => request.SecondUserId)
            .IsRequired();

        builder.Property(request => request.RequestedByUserId)
            .IsRequired();

        builder.Property(request => request.Status)
            .HasConversion<EnumToStringConverter<FriendRequestStatus>>()
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(request => request.FirstUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(request => request.SecondUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(request => request.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(request => new
            {
                request.FirstUserId,
                request.SecondUserId
            })
            .IsUnique();

        builder.HasIndex(request => new
        {
            request.RequestedByUserId,
            request.Status
        });

        builder.HasIndex(request => new
        {
            request.SecondUserId,
            request.Status
        });

        builder.ToTable("FriendRequests", table =>
        {
            table.HasCheckConstraint(
                "CK_FriendRequests_DistinctUsers",
                "\"FirstUserId\" <> \"SecondUserId\"");

            table.HasCheckConstraint(
                "CK_FriendRequests_RequesterIsParticipant",
                "\"RequestedByUserId\" = \"FirstUserId\" OR \"RequestedByUserId\" = \"SecondUserId\"");

            table.HasCheckConstraint(
                "CK_FriendRequests_ValidStatus",
                "\"Status\" IN ('Pending', 'Accepted', 'Rejected')");
        });
    }
}