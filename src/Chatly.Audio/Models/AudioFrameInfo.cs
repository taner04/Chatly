namespace Chatly.Audio.Models;

public readonly record struct AudioFrameInfo
{
    public AudioFrameInfo(ulong sequenceNumber, long framePosition)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(framePosition);

        SequenceNumber = sequenceNumber;
        FramePosition = framePosition;
    }

    public ulong SequenceNumber { get; }

    public long FramePosition { get; }
}