using Chatly.WebApi.Features.Chats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class DirectChatConfiguration : IEntityTypeConfiguration<DirectChat>
{
    public void Configure(EntityTypeBuilder<DirectChat> builder)
    {
        builder.ToTable("DirectChats");

        builder.Property(chat => chat.ParticipantKey)
            .IsRequired()
            .HasMaxLength(DirectChat.ParticipantKeyLength);

        builder.HasIndex(chat => chat.ParticipantKey)
            .IsUnique();
    }
}