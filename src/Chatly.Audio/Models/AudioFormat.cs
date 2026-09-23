namespace Chatly.Audio.Models;

public readonly record struct AudioFormat
{
    public AudioFormat(int sampleRate, int channelCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(channelCount);

        SampleRate = sampleRate;
        ChannelCount = channelCount;
    }

    public static AudioFormat Voice { get; } = new(48_000, 1);

    public int SampleRate { get; }

    public int ChannelCount { get; }
}