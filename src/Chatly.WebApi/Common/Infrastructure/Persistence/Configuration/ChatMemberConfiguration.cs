using Chatly.WebApi.Features.Chats.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatMemberId = Chatly.WebApi.Features.Chats.Models.ChatMemberId;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class ChatMemberConfiguration
    : EntityConfiguration<ChatMember, ChatMemberId>
{
    protected override void PostConfigure(EntityTypeBuilder<ChatMember> builder)
    {
        builder.HasAlternateKey(member => new { member.ChatId, member.UserId });

        builder.Property(member => member.Role)
            .HasConversion<string>();

        builder.HasOne(member => member.Chat)
            .WithMany(chat => chat.Members)
            .HasForeignKey(member => member.ChatId);

        builder.HasOne(member => member.User)
            .WithMany(user => user.ChatMemberships)
            .HasForeignKey(member => member.UserId);
    }
}