using Chatly.Desktop.Abstraction.Audio;

namespace Chatly.Desktop.Services.Audio;

[SingletonService(typeof(IAudioHost))]
internal sealed class SoundFlowAudioHost(
    IAudioAssetPlayer assetPlayer,
    SoundFlowRuntime runtime) : IAudioHost
{
    public IAudioAssetPlayer AssetPlayer { get; } = assetPlayer;

    public ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        runtime.RefreshAudioDevices();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        runtime.Dispose();
        return ValueTask.CompletedTask;
    }
}