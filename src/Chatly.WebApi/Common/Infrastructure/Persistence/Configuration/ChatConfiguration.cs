using Chatly.WebApi.Features.Chats.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class ChatConfiguration : EntityConfiguration<Chat, ChatId>
{
    protected override void PostConfigure(EntityTypeBuilder<Chat> builder)
    {
        builder.Property(chat => chat.FirstUserId)
            .IsRequired();

        builder.Property(chat => chat.SecondUserId)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(chat => chat.FirstUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(chat => chat.SecondUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(chat => new
            {
                chat.FirstUserId,
                chat.SecondUserId
            })
            .IsUnique();

        builder.ToTable("Chats", table =>
        {
            table.HasCheckConstraint(
                "CK_Chats_DistinctUsers",
                "\"FirstUserId\" <> \"SecondUserId\"");
        });
    }
}