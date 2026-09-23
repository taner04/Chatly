namespace Chatly.Audio.Abstractions;

public interface IAudioPlayback : IDisposable
{
    bool IsPlaying { get; }

    Task Completion { get; }

    ValueTask StopAsync(CancellationToken cancellationToken = default);
}