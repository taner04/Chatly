using SoundFlow.Interfaces;
using SoundFlow.Metadata.Models;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace Chatly.Audio.Services.SoundFlow;

internal sealed class BufferedQueueDataProvider : ISoundDataProvider
{
    private readonly QueueDataProvider _inner;
    private readonly int _prebufferSamples;
    private int _buffering = 1;

    internal BufferedQueueDataProvider(
        AudioFormat format,
        int prebufferSamples,
        int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(prebufferSamples);
        if (capacity < prebufferSamples)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                "The queue capacity must be at least the prebuffer size.");
        }

        _prebufferSamples = prebufferSamples;
        _inner = new QueueDataProvider(
            format,
            capacity,
            QueueFullBehavior.Throw);
    }

    public int Position => _inner.Position;

    public int Length => _inner.Length;

    public bool CanSeek => _inner.CanSeek;

    public SampleFormat SampleFormat => _inner.SampleFormat;

    public int SampleRate => _inner.SampleRate;

    public bool IsDisposed => _inner.IsDisposed;

    public SoundFormatInfo? FormatInfo => _inner.FormatInfo;

    internal int SamplesAvailable => _inner.SamplesAvailable;

    public event EventHandler<EventArgs>? EndOfStreamReached
    {
        add => _inner.EndOfStreamReached += value;
        remove => _inner.EndOfStreamReached -= value;
    }

    public event EventHandler<PositionChangedEventArgs>? PositionChanged
    {
        add => _inner.PositionChanged += value;
        remove => _inner.PositionChanged -= value;
    }

    internal void AddSamples(ReadOnlySpan<float> samples) =>
        _inner.AddSamples(samples);

    public int ReadBytes(Span<float> buffer)
    {
        if (IsDisposed || buffer.IsEmpty)
        {
            return 0;
        }

        if (Volatile.Read(ref _buffering) != 0)
        {
            if (_inner.SamplesAvailable < _prebufferSamples)
            {
                buffer.Clear();
                return buffer.Length;
            }

            Volatile.Write(ref _buffering, 0);
        }

        var samplesRead = _inner.ReadBytes(buffer);
        if (samplesRead < buffer.Length)
        {
            buffer[samplesRead..].Clear();
            Volatile.Write(ref _buffering, 1);
        }

        return buffer.Length;
    }

    public void Reset()
    {
        Volatile.Write(ref _buffering, 1);
        _inner.Reset();
    }

    public void Seek(int offset) => _inner.Seek(offset);

    public void Dispose() => _inner.Dispose();
}
