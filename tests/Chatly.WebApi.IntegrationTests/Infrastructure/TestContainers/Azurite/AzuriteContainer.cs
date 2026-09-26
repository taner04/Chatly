using Testcontainers.Azurite;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.TestContainers.Azurite;

public sealed class AzuriteContainer : ContainerBase<Testcontainers.Azurite.AzuriteContainer>
{
    public string ConnectionString => Container.GetConnectionString();

    protected override Testcontainers.Azurite.AzuriteContainer BuildContainer() =>
        new AzuriteBuilder(TestSettings.AzuriteImage)
            .WithInMemoryPersistence()
            .WithCleanUp(true)
            .Build();
}
