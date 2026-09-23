namespace Chatly.Audio.Abstractions;

public interface IAudioHost : IAsyncDisposable
{
    IAudioDeviceManager Devices { get; }

    IAudioAssetPlayer AssetPlayer { get; }

    IRealtimeAudioService RealtimeAudio { get; }

    ValueTask InitializeAsync(CancellationToken cancellationToken = default);
}