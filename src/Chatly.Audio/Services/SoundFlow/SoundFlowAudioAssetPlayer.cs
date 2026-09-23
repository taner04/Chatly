using Chatly.Audio.Abstractions;
using Chatly.Audio.Models;
using Chatly.DependencyInjection;

namespace Chatly.Audio.Services.SoundFlow;

[SingletonService(typeof(IAudioAssetPlayer))]
internal sealed class SoundFlowAudioAssetPlayer(
    SoundFlowRuntime runtime,
    IAudioDeviceManager deviceManager) : IAudioAssetPlayer
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
                playback = runtime.CreatePlayback(
                    ownedAudio,
                    deviceManager.SelectedOutputDevice?.Id,
                    options);
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
