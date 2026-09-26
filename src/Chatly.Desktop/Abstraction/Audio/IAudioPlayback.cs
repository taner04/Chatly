namespace Chatly.Desktop.Abstraction.Audio;

public interface IAudioPlayback : IDisposable
{
    Task Completion { get; }

    ValueTask StopAsync(CancellationToken cancellationToken = default);
}