using Chatly.WebApi.Common.Shared.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Interceptors;

[ScopedService(typeof(ISaveChangesInterceptor))]
internal sealed partial class AuditableInterceptor(
    CurrentUserService currentUserService,
    ILogger<AuditableInterceptor> logger)
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
            LogUnauthenticatedAudit();
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

    [LoggerMessage(
        LogLevel.Warning,
        "Unable to retrieve user ID for auditing. Setting 'CreatedBy'/'UpdatedBy' to 'system'.")]
    private partial void LogUnauthenticatedAudit();
}
