using Chatly.WebApi.Features.Chats.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class ChatReadStateConfiguration
    : EntityConfiguration<ChatReadState, ChatReadStateId>
{
    protected override void PostConfigure(EntityTypeBuilder<ChatReadState> builder)
    {
        builder.Property(state => state.ChatId)
            .IsRequired();

        builder.Property(state => state.UserId)
            .IsRequired();

        builder.Property(state => state.LastReadAt)
            .IsRequired();

        builder.HasOne<Chat>()
            .WithMany()
            .HasForeignKey(state => state.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(state => state.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(state => new
            {
                state.ChatId,
                state.UserId
            })
            .IsUnique();
    }
}
