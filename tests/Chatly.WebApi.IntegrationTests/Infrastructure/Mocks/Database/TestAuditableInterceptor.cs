using Chatly.WebApi.Common.Shared.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Database;

public sealed class TestAuditableInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            foreach (var entry in eventData.Context.ChangeTracker.Entries<Auditable>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.SetCreated("tests");
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}