using System.IO;
using Chatly.Desktop.Abstraction.Audio;
using Chatly.Desktop.Models.Audio;

namespace Chatly.Desktop.Services.Audio;

[SingletonService(typeof(IAudioAssetPlayer))]
internal sealed class SoundFlowAudioAssetPlayer(SoundFlowRuntime runtime) : IAudioAssetPlayer
{
    public async ValueTask<IAudioPlayback> StartAsync(
        Stream audioStream,
        AudioPlaybackOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(audioStream);
        if (!audioStream.CanRead)
        {
            throw new ArgumentException("The audioStream stream must be readable.", nameof(audioStream));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var ownedAudio = new MemoryStream();
        try
        {
            await audioStream.CopyToAsync(ownedAudio, cancellationToken);
            ownedAudio.Position = 0;

            SoundFlowAudioPlayback? playback = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                playback = runtime.CreatePlayback(ownedAudio, options);
                playback.Start();
                return playback;
            }
            catch
            {
                playback?.Dispose();
                throw;
            }
        }
        catch
        {
            await ownedAudio.DisposeAsync();
            throw;
        }
    }
}