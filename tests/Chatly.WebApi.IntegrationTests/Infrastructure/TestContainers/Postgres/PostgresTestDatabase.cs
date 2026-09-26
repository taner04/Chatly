using Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Database;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.TestContainers.Postgres;

public sealed class PostgresTestDatabase : IAsyncDisposable
{
    private readonly PostgresContainer _container = new();
    private DbContextOptions<ChatlyDbContext> _options = null!;
    private string _truncateSql = null!;

    public string ConnectionString => _container.ConnectionString;

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public async Task InitializeAsync()
    {
        await _container.InitializeAsync();

        _options = new DbContextOptionsBuilder<ChatlyDbContext>()
            .UseNpgsql(_container.ConnectionString)
            .AddInterceptors(new TestAuditableInterceptor())
            .Options;

        await using var context = CreateDbContext();
        await context.Database.MigrateAsync(TestContext.Current.CancellationToken);

        var tables = context.Model.GetEntityTypes()
            .Select(entityType => entityType.GetTableName())
            .OfType<string>()
            .Distinct()
            .Select(table => $"\"{table}\"");
        _truncateSql = $"TRUNCATE TABLE {string.Join(", ", tables)} CASCADE;";
    }

    public async Task ResetAsync()
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlRawAsync(_truncateSql, TestContext.Current.CancellationToken);
    }

    public ChatlyDbContext CreateDbContext() => new(_options);
}
