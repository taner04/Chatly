using System.Data;

namespace Chatly.WebApi.Common.Extensions;

internal static class DbContextTransactionExtensions
{
    extension(DbContext context)
    {
        internal async Task<T> ExecuteSerializableAsync<T>(
            Func<Task<T>> operation,
            CancellationToken cancellationToken)
        {
            var strategy = context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                context.ChangeTracker.Clear();
                await using var transaction = await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
                var result = await operation();
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            });
        }
    }
}