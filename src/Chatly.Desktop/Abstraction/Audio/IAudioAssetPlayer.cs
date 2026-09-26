using System.IO;
using Chatly.Desktop.Models.Audio;

namespace Chatly.Desktop.Abstraction.Audio;

public interface IAudioAssetPlayer
{
    ValueTask<IAudioPlayback> StartAsync(
        Stream audioStream,
        AudioPlaybackOptions? options = null,
        CancellationToken cancellationToken = default);
}