using Chatly.Audio.Enums;
using Chatly.Audio.Events;
using Chatly.Audio.Models;

namespace Chatly.Audio.Abstractions;

public interface IRealtimeAudioSession : IAsyncDisposable
{
    AudioFormat Format { get; }

    AudioSessionState State { get; }

    event EventHandler<AudioSessionFaultedEventArgs>? Faulted;

    ValueTask StartAsync(CancellationToken cancellationToken = default);

    ValueTask StopAsync(CancellationToken cancellationToken = default);

    // Implementations must copy accepted samples before returning and must not block the caller.
    bool TryWritePlayback(ReadOnlySpan<float> interleavedSamples);

    void ClearPlayback();

    event Action<ReadOnlyMemory<float>>? AudioCaptured;
}
