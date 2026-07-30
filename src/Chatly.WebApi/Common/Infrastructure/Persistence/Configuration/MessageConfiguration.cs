using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.Messages.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class MessageConfiguration : EntityConfiguration<Message, MessageId>
{
    protected override void PostConfigure(EntityTypeBuilder<Message> builder)
    {
        builder.Property(message => message.Content)
            .IsRequired();

        builder.HasOne(message => message.Sender)
            .WithMany()
            .HasForeignKey(message => message.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ChatMember>()
            .WithMany()
            .HasForeignKey(message => new { message.ChatId, message.SenderId })
            .HasPrincipalKey(member => new { member.ChatId, member.UserId })
            .OnDelete(DeleteBehavior.Restrict);
    }
}