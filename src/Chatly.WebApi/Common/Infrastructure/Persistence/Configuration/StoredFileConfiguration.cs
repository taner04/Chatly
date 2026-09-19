using Chatly.WebApi.Features.StoredFiles.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

internal sealed class StoredFileConfiguration : EntityConfiguration<StoredFile, StoredFileId>
{
    protected override void PostConfigure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.Property(file => file.UserId)
            .IsRequired();

        builder.Property(file => file.BlobName)
            .IsRequired()
            .HasMaxLength(StoredFile.MaxBlobNameLength);

        builder.Property(file => file.FileName)
            .IsRequired()
            .HasMaxLength(StoredFile.MaxFileNameLength);

        builder.Property(file => file.ContentType)
            .IsRequired()
            .HasMaxLength(StoredFile.MaxContentTypeLength);

        builder.Property(file => file.Size)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(file => file.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(file => file.BlobName)
            .IsUnique();

        builder.HasIndex(file => file.UserId);

        builder.ToTable("StoredFiles", table =>
        {
            table.HasCheckConstraint(
                "CK_StoredFiles_NonNegativeSize",
                "\"Size\" >= 0");
        });
    }
}