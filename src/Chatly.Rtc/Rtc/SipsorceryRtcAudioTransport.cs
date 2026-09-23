using System.Threading.Channels;
using Chatly.Rtc.Abstractions;

namespace Chatly.Rtc.Rtc;

internal sealed class SipsorceryRtcAudioTransport : IRtcAudioTransport
{
    private readonly Action<float[]> _send;
    private readonly Channel<ReadOnlyMemory<float>> _received = Channel.CreateBounded<ReadOnlyMemory<float>>(
        new BoundedChannelOptions(16)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true
        });
    private readonly Task _deliveryTask;
    private int _closed;

    internal SipsorceryRtcAudioTransport(Action<float[]> send)
    {
        _send = send;
        _deliveryTask = Task.Run(DeliverReceivedAsync);
    }

    public event Action<ReadOnlyMemory<float>>? AudioReceived;

    public void Send(ReadOnlySpan<float> samples)
    {
        if (samples.IsEmpty || Volatile.Read(ref _closed) != 0)
        {
            return;
        }

        _send([.. samples]);
    }

    internal void PublishReceived(ReadOnlyMemory<float> samples)
    {
        if (Volatile.Read(ref _closed) != 0)
        {
            return;
        }

        _received.Writer.TryWrite(samples);
    }

    internal Task CloseAsync()
    {
        Interlocked.Exchange(ref _closed, 1);
        _received.Writer.TryComplete();
        return _deliveryTask;
    }

    private async Task DeliverReceivedAsync()
    {
        await foreach (var samples in _received.Reader.ReadAllAsync())
        {
            var subscribers = AudioReceived?.GetInvocationList();
            if (subscribers is null)
            {
                continue;
            }

            foreach (var subscriber in subscribers)
            {
                try
                {
                    ((Action<ReadOnlyMemory<float>>)subscriber)(samples);
                }
                catch
                {
                    // One playback consumer must not stop delivery to the others.
                }
            }
        }

        AudioReceived = null;
    }
}
