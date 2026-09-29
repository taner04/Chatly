using DotNet.Testcontainers.Containers;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.TestContainers;

public abstract class ContainerBase<T> : IAsyncLifetime where T : DockerContainer
{
    private const int MaxRetryAttempts = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected T Container = null!;

    public async ValueTask InitializeAsync()
    {
        Container = BuildContainer();

        for (var attempt = 1;; attempt++)
        {
            try
            {
                await Container.StartAsync(TestContext.Current.CancellationToken);
                return;
            }
            catch (Exception) when (attempt < MaxRetryAttempts)
            {
                await Task.Delay(RetryDelay, TestContext.Current.CancellationToken);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Container.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected abstract T BuildContainer();
}