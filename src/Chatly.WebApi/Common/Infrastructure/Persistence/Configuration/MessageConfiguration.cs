using Chatly.WebApi.Features.Messages.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class MessageConfiguration : EntityConfiguration<Message, MessageId>
{
    protected override void PostConfigure(EntityTypeBuilder<Message> builder)
    {
        builder.Property(message => message.ChatId)
            .IsRequired();

        builder.Property(message => message.SenderUserId)
            .IsRequired();

        builder.Property(message => message.Content)
            .IsRequired()
            .HasMaxLength(Message.MaxContentLength);

        builder.Property(message => message.SentAt)
            .IsRequired();

        builder.HasOne(message => message.Chat)
            .WithMany(chat => chat.Messages)
            .HasForeignKey(message => message.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(message => message.SenderUser)
            .WithMany()
            .HasForeignKey(message => message.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(message => new
        {
            message.ChatId,
            message.SentAt,
            message.Id
        });
    }
}