using Chatly.Audio.Models;

namespace Chatly.Audio.Abstractions;

public interface IRealtimeAudioService
{
    ValueTask<IRealtimeAudioSession> CreateSessionAsync(
        RealtimeAudioOptions options,
        CancellationToken cancellationToken = default);
}