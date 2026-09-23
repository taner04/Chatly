using Chatly.Rtc.Rtc;

namespace Chatly.Rtc.Tests.Rtc;

public sealed class RtcAudioProcessingTests
{
    [Theory]
    [InlineData(-2f, short.MinValue)]
    [InlineData(-1f, short.MinValue)]
    [InlineData(0f, (short)0)]
    [InlineData(1f, short.MaxValue)]
    [InlineData(2f, short.MaxValue)]
    public void FloatToPcm16_ClampsBoundaries(float sample, short expected)
    {
        Assert.Equal(expected, RtcAudioProcessing.FloatToPcm16(sample));
    }

    [Fact]
    public void FrameConstants_RepresentTwentyMillisecondsAt48Khz()
    {
        Assert.Equal(48_000, RtcAudioProcessing.SampleRate);
        Assert.Equal(20, RtcAudioProcessing.FrameDurationMilliseconds);
        Assert.Equal(960, RtcAudioProcessing.SamplesPerFrame);
    }

    [Theory]
    [InlineData(960, 48_000, 48_000, 960u)]
    [InlineData(160, 8_000, 8_000, 160u)]
    [InlineData(960, 48_000, 8_000, 160u)]
    public void CalculateRtpDuration_UsesNegotiatedClock(
        int sampleCount,
        int sampleRate,
        int clockRate,
        uint expected)
    {
        Assert.Equal(expected, RtcAudioProcessing.CalculateRtpDuration(sampleCount, sampleRate, clockRate));
    }

    [Fact]
    public void Resample_ConvertsTwentyMillisecondsFrom48KhzTo8Khz()
    {
        var input = Enumerable.Repeat((short)1_000, RtcAudioProcessing.SamplesPerFrame).ToArray();

        var output = RtcAudioProcessing.Resample(input, 48_000, 8_000);

        Assert.Equal(160, output.Length);
    }

    [Fact]
    public void Pcm16ToFloat_MapsSignedPcmRange()
    {
        Assert.Equal(-1f, RtcAudioProcessing.Pcm16ToFloat(short.MinValue));
        Assert.Equal(short.MaxValue / 32768f, RtcAudioProcessing.Pcm16ToFloat(short.MaxValue));
    }
}
