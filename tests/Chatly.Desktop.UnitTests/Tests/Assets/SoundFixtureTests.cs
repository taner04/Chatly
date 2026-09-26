using System.Text;
using Avalonia.Platform;
using Chatly.Desktop.UnitTests.Infrastructure;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Providers;

namespace Chatly.Desktop.UnitTests.Tests.Assets;

public sealed class SoundFixtureTests
{
    [Theory]
    [InlineData("notification.wav")]
    [InlineData("ringtone.wav")]
    public Task WaveAsset_Should_HaveRiffSignatureAndDecode_When_Loaded(string fileName) => UiThread.RunAsync(() =>
    {
        var asset = new Uri($"avares://Chatly.Desktop/Assets/{fileName}");

        using (var header = AssetLoader.Open(asset))
        {
            Span<byte> bytes = stackalloc byte[12];
            header.ReadExactly(bytes);

            Encoding.ASCII.GetString(bytes[..4]).Should().Be("RIFF");
            Encoding.ASCII.GetString(bytes[8..12]).Should().Be("WAVE");
        }

        using var stream = AssetLoader.Open(asset);
        using var engine = new MiniAudioEngine();
        using var provider = new AssetDataProvider(engine, stream);
        var samples = new float[4_096];

        var samplesRead = provider.ReadBytes(samples);

        provider.Length.Should().BeGreaterThan(0);
        samplesRead.Should().BeGreaterThan(0);
        return Task.CompletedTask;
    });
}
