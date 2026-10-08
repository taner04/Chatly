using Chatly.WebApi.Features.DeviceSessions.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class DeviceSessionConfiguration : EntityConfiguration<DeviceSession, DeviceSessionId>
{
    protected override void PostConfigure(EntityTypeBuilder<DeviceSession> builder)
    {
        builder.Property(session => session.UserId)
            .IsRequired();

        builder.Property(session => session.DeviceId)
            .IsRequired();

        builder.Property(session => session.DeviceName)
            .IsRequired()
            .HasMaxLength(DeviceSession.MaxDeviceNameLength);

        builder.Property(session => session.Platform)
            .IsRequired()
            .HasMaxLength(DeviceSession.MaxPlatformLength);

        builder.Property(session => session.AppVersion)
            .IsRequired()
            .HasMaxLength(DeviceSession.MaxAppVersionLength);

        builder.Property(session => session.LastSeenAt)
            .IsRequired();

        builder.Property(session => session.IdentitySessionId)
            .HasMaxLength(DeviceSession.MaxIdentitySessionIdLength);

        builder.Property(session => session.RevokedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(session => new
            {
                session.UserId,
                session.DeviceId
            })
            .IsUnique();

        builder.HasIndex(session => session.IdentitySessionId);
    }
}