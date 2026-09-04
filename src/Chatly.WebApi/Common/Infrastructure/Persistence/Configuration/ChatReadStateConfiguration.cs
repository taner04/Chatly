using Chatly.WebApi.Features.Chats.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class ChatReadStateConfiguration : IEntityTypeConfiguration<ChatReadState>
{
    public void Configure(EntityTypeBuilder<ChatReadState> builder)
    {
        builder.HasKey(state => new
        {
            state.ChatId,
            state.UserId
        });

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
    }
}
