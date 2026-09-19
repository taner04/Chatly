using Chatly.WebApi.Features.MessageAttachments.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class MessageAttachmentConfiguration
    : EntityConfiguration<MessageAttachment, MessageAttachmentId>
{
    protected override void PostConfigure(EntityTypeBuilder<MessageAttachment> builder)
    {
        builder.Property(attachment => attachment.MessageId)
            .IsRequired();

        builder.Property(attachment => attachment.StoredFileId)
            .IsRequired();

        builder.HasOne<Message>()
            .WithMany(message => message.Attachments)
            .HasForeignKey(attachment => attachment.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(attachment => attachment.StoredFile)
            .WithMany()
            .HasForeignKey(attachment => attachment.StoredFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(attachment => attachment.StoredFileId)
            .IsUnique();

        builder.HasIndex(attachment => new
            {
                attachment.MessageId,
                attachment.StoredFileId
            })
            .IsUnique();
    }
}