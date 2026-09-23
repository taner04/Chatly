using Chatly.Contracts.Common.Policies;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class UserConfiguration : EntityConfiguration<User, UserId>
{
    protected override void PostConfigure(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.Email)
            .HasColumnType(PostgresDataType.CaseInsensitiveText)
            .IsRequired()
            .HasMaxLength(User.MaxEmailLength);

        builder.Property(user => user.Auth0Id)
            .IsRequired()
            .HasMaxLength(User.MaxAuth0IdLength);

        builder.Property(user => user.Username)
            .HasColumnType(PostgresDataType.CaseInsensitiveText)
            .HasMaxLength(UsernamePolicy.MaxLength);

        builder.Property(user => user.OnboardingCompleted)
            .IsRequired();

        builder.Property(user => user.LastSeenAt);
        builder.Property(user => user.LastAbsenceEmailAt);

        builder.HasOne(user => user.ProfilePictureFile)
            .WithMany()
            .HasForeignKey(user => user.ProfilePictureFileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.HasIndex(user => user.Auth0Id)
            .IsUnique();

        builder.HasIndex(user => user.Username)
            .IsUnique()
            .HasFilter("\"Username\" IS NOT NULL");

        builder.HasIndex(user => user.ProfilePictureFileId)
            .IsUnique()
            .HasFilter("\"ProfilePictureFileId\" IS NOT NULL");

        builder.HasIndex(user => new
            {
                user.LastSeenAt,
                user.LastAbsenceEmailAt
            })
            .HasFilter("\"OnboardingCompleted\" = TRUE AND \"LastSeenAt\" IS NOT NULL");
    }
}
