using Chatly.WebApi.Common.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Interceptors;

public sealed class AuditableInterceptor(CurrentUserService currentUserService, ILogger<AuditableInterceptor> logger)
    : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            SetAuditableProperties(eventData.Context);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetAuditableProperties(
        DbContext context)
    {
        var auditableEntries = context.ChangeTracker
            .Entries<Auditable>()
            .ToList();

        var changeMadeBy = "system";
        try
        {
            changeMadeBy = currentUserService.GetCurrentUserId().Value.ToString();
        }
        catch (UnauthorizedAccessException)
        {
            // If the user is not authenticated, we can choose to set a default value or skip setting the properties.
            // Here, we set it to "system" to indicate that the change was made by an unauthenticated user or a system process.
            logger.LogWarning("Unable to retrieve user ID for auditing. Setting 'CreatedBy'/'UpdatedBy' to 'system'.");
        }

        foreach (var entry in auditableEntries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                {
                    entry.Entity.SetCreated(changeMadeBy);
                    break;
                }
                case EntityState.Modified:
                {
                    entry.Entity.SetUpdated(changeMadeBy);
                    break;
                }
                case EntityState.Deleted:
                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    break;
            }
        }
    }
}