using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.Users.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class UserConfiguration : EntityConfiguration<User, UserId>
{
    protected override void PostConfigure(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(user => user.Auth0Id)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.HasIndex(user => user.Auth0Id)
            .IsUnique();

        builder.HasMany(user => user.Chats)
            .WithMany(chat => chat.Users)
            .UsingEntity<ChatMember>(
                membership => membership
                    .HasOne(member => member.Chat)
                    .WithMany(chat => chat.Members)
                    .HasForeignKey(member => member.ChatId),
                membership => membership
                    .HasOne(member => member.User)
                    .WithMany(user => user.ChatMemberships)
                    .HasForeignKey(member => member.UserId));
    }
}
