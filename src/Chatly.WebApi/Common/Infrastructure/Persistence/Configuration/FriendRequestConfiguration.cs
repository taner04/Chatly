using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.FriendRequests.Models;
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

        builder.Property(request => request.SenderUserId)
            .IsRequired();

        builder.Property(request => request.ReceiverUserId)
            .IsRequired();

        builder.Property(request => request.Status)
            .HasConversion<EnumToStringConverter<FriendRequestStatus>>()
            .IsRequired();

        builder.HasOne(request => request.SenderUser)
            .WithMany(user => user.SentFriendRequests)
            .HasForeignKey(request => request.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(request => request.ReceiverUser)
            .WithMany(user => user.ReceivedFriendRequests)
            .HasForeignKey(request => request.ReceiverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(request => new
            {
                request.FirstUserId,
                request.SecondUserId
            })
            .IsUnique();

        builder.HasIndex(request => new
        {
            request.SenderUserId,
            request.Status
        });

        builder.HasIndex(request => new
        {
            request.ReceiverUserId,
            request.Status
        });

        builder.ToTable("FriendRequests", table =>
        {
            table.HasCheckConstraint(
                "CK_FriendRequests_DistinctUsers",
                "\"FirstUserId\" <> \"SecondUserId\"");

            table.HasCheckConstraint(
                "CK_FriendRequests_ValidStatus",
                "\"Status\" IN ('Pending', 'Accepted', 'Rejected')");
        });
    }
}