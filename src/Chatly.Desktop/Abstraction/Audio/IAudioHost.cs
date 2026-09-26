namespace Chatly.Desktop.Abstraction.Audio;

public interface IAudioHost : IAsyncDisposable
{
    IAudioAssetPlayer AssetPlayer { get; }

    ValueTask InitializeAsync(CancellationToken cancellationToken = default);
}