namespace Chatly.WebApi.Common.Shared.Models;

public abstract class Auditable
{
    public const int MaxCreatedByLength = 256;
    public const int MaxUpdatedByLength = 256;

    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = null!;

    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }


    public void SetCreated(string? createdBy = null!)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedBy = createdBy ?? "System";
    }

    public void SetUpdated(string? updatedBy = null!)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy ?? "System";
    }
}