using Chatly.WebApi.Features.Chats.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatId = Chatly.WebApi.Features.Chats.Models.ChatId;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class ChatConfiguration : EntityConfiguration<Chat, ChatId>
{
    protected override void PostConfigure(EntityTypeBuilder<Chat> builder)
    {
        builder.HasMany(chat => chat.Messages)
            .WithOne(message => message.Chat)
            .HasForeignKey(message => message.ChatId);
    }
}