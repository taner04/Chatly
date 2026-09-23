using System.Text;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Providers;

namespace Chatly.Audio.Tests.Integration;

public sealed class SoundFixtureTests
{
    [Theory]
    [InlineData("notification.wav")]
    [InlineData("ringtone.wav")]
    public void DesktopWaveFixtureHasSignatureAndDecodesSamples(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);
        Assert.True(File.Exists(path), $"Linked fixture was not copied: {path}");

        using var stream = File.OpenRead(path);
        Span<byte> header = stackalloc byte[12];
        stream.ReadExactly(header);

        Assert.Equal("RIFF", Encoding.ASCII.GetString(header[..4]));
        Assert.Equal("WAVE", Encoding.ASCII.GetString(header[8..12]));

        stream.Position = 0;
        using var engine = new MiniAudioEngine();
        using var provider = new AssetDataProvider(engine, stream);
        var samples = new float[4_096];

        var samplesRead = provider.ReadBytes(samples);

        Assert.True(provider.Length > 0);
        Assert.True(samplesRead > 0);
    }
}
