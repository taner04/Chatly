using Testcontainers.PostgreSql;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.TestContainers.Postgres;

public sealed class PostgresContainer : ContainerBase<PostgreSqlContainer>
{
    public string ConnectionString => Container.GetConnectionString();

    protected override PostgreSqlContainer BuildContainer() =>
        new PostgreSqlBuilder(TestSettings.PostgresImage)
            .WithCleanUp(true)
            .Build();
}