using Chatly.WebApi.Features.Calls.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class ActiveCallParticipantConfiguration
    : EntityConfiguration<ActiveCallParticipant, ActiveCallParticipantId>
{
    protected override void PostConfigure(EntityTypeBuilder<ActiveCallParticipant> builder)
    {
        builder.Property(activeCall => activeCall.UserId)
            .IsRequired();

        builder.Property(activeCall => activeCall.CallId)
            .IsRequired();

        builder.HasOne(activeCall => activeCall.User)
            .WithMany()
            .HasForeignKey(activeCall => activeCall.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(activeCall => activeCall.Call)
            .WithMany(call => call.ActiveParticipants)
            .HasForeignKey(activeCall => activeCall.CallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(activeCall => activeCall.UserId)
            .IsUnique();

        builder.HasIndex(activeCall => new
            {
                activeCall.CallId,
                activeCall.UserId
            })
            .IsUnique();
    }
}