using Chatly.WebApi.Common.Shared.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Configuration;

public abstract class EntityConfiguration<TEntity, TId> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity<TId>
    where TId : struct
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Entity configuration
        builder.ToTable(typeof(TEntity).Name + "s");
        builder.HasKey(t => t.Id);

        // IAuditable configuration
        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(Auditable.MaxCreatedByLength);

        builder.Property(t => t.UpdatedAt);

        builder.Property(t => t.UpdatedBy)
            .HasMaxLength(Auditable.MaxUpdatedByLength);

        PostConfigure(builder);
    }

    /// <summary>
    ///     Hook method for additional configurations in derived classes.
    ///     Executed after the base configuration to add entity-specific settings.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{TEntity}" /> for further configurations.</param>
    protected abstract void PostConfigure(
        EntityTypeBuilder<TEntity> builder);

    protected class PostgresDataType
    {
        public const string CaseInsensitiveText = "citext";
    }
}