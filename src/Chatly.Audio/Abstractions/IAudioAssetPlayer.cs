using Chatly.Audio.Models;

namespace Chatly.Audio.Abstractions;

public interface IAudioAssetPlayer
{
    ValueTask<IAudioPlayback> StartAsync(
        Stream audioStream,
        AudioPlaybackOptions? options = null,
        CancellationToken cancellationToken = default);
}
