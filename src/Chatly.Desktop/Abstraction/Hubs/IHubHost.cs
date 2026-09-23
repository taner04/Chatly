namespace Chatly.Desktop.Abstraction.Hubs;

public interface IHubHost
{
    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}