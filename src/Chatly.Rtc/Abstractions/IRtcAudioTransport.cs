namespace Chatly.Rtc.Abstractions;

public interface IRtcAudioTransport
{
    void Send(ReadOnlySpan<float> samples);

    event Action<ReadOnlyMemory<float>>? AudioReceived;
}