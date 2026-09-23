using SIPSorcery.Media;

namespace Chatly.Rtc.Rtc;

internal static class RtcAudioProcessing
{
    internal const int SampleRate = 48_000;
    internal const int FrameDurationMilliseconds = 20;
    internal const int SamplesPerFrame = SampleRate * FrameDurationMilliseconds / 1_000;

    internal static short FloatToPcm16(float sample) => sample switch
    {
        <= -1f => short.MinValue,
        >= 1f => short.MaxValue,
        _ => (short)Math.Round(sample * short.MaxValue)
    };

    internal static float Pcm16ToFloat(short sample) => sample / 32768f;

    internal static uint CalculateRtpDuration(int sampleCount, int sampleRate, int rtpClockRate) =>
        (uint)Math.Round(
            sampleCount * (double)rtpClockRate / sampleRate,
            MidpointRounding.AwayFromZero);

    internal static short[] Resample(short[] samples, int sourceRate, int targetRate) =>
        PcmResampler.Resample(samples, sourceRate, targetRate);
}
