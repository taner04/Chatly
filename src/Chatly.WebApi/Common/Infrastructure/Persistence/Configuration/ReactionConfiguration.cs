using Chatly.Contracts.Features.Reactions.Models;
using Chatly.WebApi.Features.Reactions.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class ReactionConfiguration : EntityConfiguration<Reaction, ReactionId>
{
    protected override void PostConfigure(EntityTypeBuilder<Reaction> builder)
    {
        var reactionTypes = string.Join(", ", Enum.GetNames<ReactionType>()
            .Select(name => $"'{name.Replace("'", "''")}'"));

        builder.Property(reaction => reaction.MessageId)
            .IsRequired();

        builder.Property(reaction => reaction.UserId)
            .IsRequired();

        builder.Property(reaction => reaction.Type)
            .HasConversion<EnumToStringConverter<ReactionType>>()
            .IsRequired();

        builder.HasOne<Message>()
            .WithMany(message => message.Reactions)
            .HasForeignKey(reaction => reaction.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(reaction => reaction.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(reaction => new
            {
                reaction.MessageId,
                reaction.UserId
            })
            .IsUnique();

        builder.ToTable("Reactions", table =>
        {
            table.HasCheckConstraint(
                "CK_Reactions_ValidType",
                $"\"Type\" IN ({reactionTypes})");
        });
    }
}