using Chatly.WebApi.Features.Chats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class GroupChatConfiguration : IEntityTypeConfiguration<GroupChat>
{
    public void Configure(EntityTypeBuilder<GroupChat> builder)
    {
        builder.ToTable("GroupChats");

        builder.Property(chat => chat.Name)
            .IsRequired()
            .HasMaxLength(GroupChat.MaxNameLength);
    }
}