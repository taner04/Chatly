using Chatly.WebApi.Features.Calls.Enums;
using Chatly.WebApi.Features.Calls.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class CallConfiguration : EntityConfiguration<Call, CallId>
{
    protected override void PostConfigure(EntityTypeBuilder<Call> builder)
    {
        var statuses = GetEnumValues<CallStatus>();
        var endReasons = GetEnumValues<CallEndReason>();

        builder.Property(call => call.CallerUserId)
            .IsRequired();

        builder.Property(call => call.ReceiverUserId)
            .IsRequired();

        builder.Property(call => call.Status)
            .HasConversion<EnumToStringConverter<CallStatus>>()
            .IsRequired();

        builder.Property(call => call.EndReason)
            .HasConversion<EnumToStringConverter<CallEndReason>>();

        builder.Property(call => call.InitiatedAt)
            .IsRequired();

        builder.Property(call => call.AcceptedAt);
        builder.Property(call => call.EndedAt);

        builder.HasOne(call => call.CallerUser)
            .WithMany()
            .HasForeignKey(call => call.CallerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(call => call.ReceiverUser)
            .WithMany()
            .HasForeignKey(call => call.ReceiverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(call => new
        {
            call.CallerUserId,
            call.InitiatedAt
        });

        builder.HasIndex(call => new
        {
            call.ReceiverUserId,
            call.InitiatedAt
        });

        builder.ToTable("Calls", table =>
        {
            table.HasCheckConstraint(
                "CK_Calls_DistinctUsers",
                "\"CallerUserId\" <> \"ReceiverUserId\"");
            table.HasCheckConstraint(
                "CK_Calls_ValidStatus",
                $"\"Status\" IN ({statuses})");
            table.HasCheckConstraint(
                "CK_Calls_ValidEndReason",
                $"\"EndReason\" IS NULL OR \"EndReason\" IN ({endReasons})");
            table.HasCheckConstraint(
                "CK_Calls_TerminalState",
                "(\"Status\" = 'Ended' AND \"EndedAt\" IS NOT NULL AND \"EndReason\" IS NOT NULL) OR " +
                "(\"Status\" <> 'Ended' AND \"EndedAt\" IS NULL AND \"EndReason\" IS NULL)");
            table.HasCheckConstraint(
                "CK_Calls_AcceptedAt",
                "(\"Status\" = 'Ringing' AND \"AcceptedAt\" IS NULL) OR " +
                "(\"Status\" IN ('Accepted', 'Offered', 'Active') AND \"AcceptedAt\" IS NOT NULL) OR " +
                "\"Status\" = 'Ended'");
            table.HasCheckConstraint(
                "CK_Calls_TimestampOrder",
                "(\"AcceptedAt\" IS NULL OR \"AcceptedAt\" >= \"InitiatedAt\") AND " +
                "(\"EndedAt\" IS NULL OR \"EndedAt\" >= \"InitiatedAt\") AND " +
                "(\"AcceptedAt\" IS NULL OR \"EndedAt\" IS NULL OR \"EndedAt\" >= \"AcceptedAt\")");
        });
    }

    private static string GetEnumValues<TEnum>() where TEnum : struct, Enum =>
        string.Join(", ", Enum.GetNames<TEnum>()
            .Select(name => $"'{name.Replace("'", "''")}'"));
}
