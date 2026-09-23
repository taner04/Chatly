using Chatly.Audio.Services.SoundFlow;
using SoundFlow.Enums;
using SoundFlow.Structs;

namespace Chatly.Audio.Tests.Services.SoundFlow;

public sealed class BufferedQueueDataProviderTests
{
    private static readonly AudioFormat StereoFormat = new()
    {
        Format = SampleFormat.F32,
        Channels = 2,
        Layout = AudioFormat.GetLayoutFromChannels(2),
        SampleRate = 48_000
    };

    [Fact]
    public void ReadReturnsSilenceWithoutConsumingUntilPrebufferIsReached()
    {
        using var provider = new BufferedQueueDataProvider(StereoFormat, 6, 12);
        provider.AddSamples([1f, 2f, 3f, 4f]);
        var output = new float[4];

        var samplesRead = provider.ReadBytes(output);

        Assert.Equal(output.Length, samplesRead);
        Assert.Equal(new float[4], output);
        Assert.Equal(4, provider.SamplesAvailable);
    }

    [Fact]
    public void ReadBeginsAfterPrebufferAndPreservesQueuedSamples()
    {
        using var provider = new BufferedQueueDataProvider(StereoFormat, 4, 8);
        provider.AddSamples([1f, 2f, 3f, 4f]);
        var output = new float[4];

        provider.ReadBytes(output);

        Assert.Equal(new[] { 1f, 2f, 3f, 4f }, output);
        Assert.Equal(0, provider.SamplesAvailable);
    }

    [Fact]
    public void UnderrunOutputsSilenceAndRearmsPrebuffering()
    {
        using var provider = new BufferedQueueDataProvider(StereoFormat, 4, 8);
        provider.AddSamples([1f, 2f, 3f, 4f]);
        var firstOutput = new float[6];
        provider.ReadBytes(firstOutput);

        provider.AddSamples([5f, 6f]);
        var secondOutput = new float[2];
        provider.ReadBytes(secondOutput);

        Assert.Equal(new[] { 1f, 2f, 3f, 4f, 0f, 0f }, firstOutput);
        Assert.Equal(new float[2], secondOutput);
        Assert.Equal(2, provider.SamplesAvailable);
    }
}
