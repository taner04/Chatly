using Chatly.WebApi.Features.Users.Models;
using Microsoft.EntityFrameworkCore;
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
            .HasMaxLength(User.MaxUsernameLength);

        builder.Property(user => user.ProfilePictureKey)
            .HasMaxLength(User.MaxProfilePictureKeyLength);

        builder.Property(user => user.OnboardingCompleted)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.HasIndex(user => user.Auth0Id)
            .IsUnique();

        builder.HasIndex(user => user.Username)
            .IsUnique()
            .HasFilter("\"Username\" IS NOT NULL");
    }
}