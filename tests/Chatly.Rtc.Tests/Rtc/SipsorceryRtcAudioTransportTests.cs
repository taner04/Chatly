using Chatly.Rtc.Rtc;

namespace Chatly.Rtc.Tests.Rtc;

public sealed class SipsorceryRtcAudioTransportTests
{
    [Fact]
    public async Task Send_CopiesCallerBufferBeforeDispatch()
    {
        float[]? dispatched = null;
        var transport = new SipsorceryRtcAudioTransport(samples => dispatched = samples);
        var callerBuffer = new[] { 0.25f, -0.5f };

        transport.Send(callerBuffer);
        callerBuffer[0] = 1f;

        Assert.Equal(new[] { 0.25f, -0.5f }, dispatched);
        await transport.CloseAsync();
    }

    [Fact]
    public async Task Send_IgnoresEmptyAndClosedInput()
    {
        var dispatchCount = 0;
        var transport = new SipsorceryRtcAudioTransport(_ => dispatchCount++);

        transport.Send([]);
        await transport.CloseAsync();
        await transport.CloseAsync();
        transport.Send([0.5f]);

        Assert.Equal(0, dispatchCount);
    }

    [Fact]
    public async Task ReceivedAudio_IsOrdered_IsolatesSubscribers_AndStopsAfterClose()
    {
        var deliveries = new List<string>();
        var transport = new SipsorceryRtcAudioTransport(_ => { });
        transport.AudioReceived += samples => deliveries.Add($"first:{samples.Span[0]}");
        transport.AudioReceived += _ => throw new InvalidOperationException("Observer failure");
        transport.AudioReceived += samples => deliveries.Add($"last:{samples.Span[0]}");

        transport.PublishReceived(new float[] { 1f });
        transport.PublishReceived(new float[] { 2f });
        await transport.CloseAsync();
        transport.PublishReceived(new float[] { 3f });

        Assert.Equal(
            ["first:1", "last:1", "first:2", "last:2"],
            deliveries);
    }
}
