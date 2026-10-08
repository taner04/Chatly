using Npgsql;

namespace Chatly.WebApi.Common.Extensions;

internal static class DbUpdateExceptionExtensions
{
    extension(DbUpdateException exception)
    {
        internal bool IsUniqueViolation(string? constraintName = null) =>
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres
            && (constraintName is null || postgres.ConstraintName == constraintName);
    }
}