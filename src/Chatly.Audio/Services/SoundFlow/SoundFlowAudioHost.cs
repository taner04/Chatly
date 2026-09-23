using Chatly.Audio.Abstractions;
using Chatly.DependencyInjection;

namespace Chatly.Audio.Services.SoundFlow;

[SingletonService(typeof(IAudioHost))]
internal sealed class SoundFlowAudioHost(
    IAudioDeviceManager devices,
    IAudioAssetPlayer assetPlayer,
    IRealtimeAudioService realtimeAudio,
    SoundFlowRuntime runtime) : IAudioHost
{
    public IAudioDeviceManager Devices { get; } = devices;

    public IAudioAssetPlayer AssetPlayer { get; } = assetPlayer;

    public IRealtimeAudioService RealtimeAudio { get; } = realtimeAudio;

    public ValueTask InitializeAsync(CancellationToken cancellationToken = default) =>
        Devices.RefreshAsync(cancellationToken);

    public ValueTask DisposeAsync()
    {
        runtime.Dispose();
        return ValueTask.CompletedTask;
    }
}
