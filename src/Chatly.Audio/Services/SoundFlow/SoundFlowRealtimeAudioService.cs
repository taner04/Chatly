using Chatly.Audio.Abstractions;
using Chatly.Audio.Models;
using Chatly.DependencyInjection;

namespace Chatly.Audio.Services.SoundFlow;

[SingletonService(typeof(IRealtimeAudioService))]
internal sealed class SoundFlowRealtimeAudioService(
    SoundFlowRuntime runtime,
    IAudioDeviceManager deviceManager) : IRealtimeAudioService
{
    private const int PlaybackChannelCount = 2;

    public ValueTask<IRealtimeAudioSession> CreateSessionAsync(
        RealtimeAudioOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        if (options.Format != AudioFormat.Voice)
        {
            throw new ArgumentException(
                $"Realtime audio requires {AudioFormat.Voice.SampleRate} Hz mono audio.",
                nameof(options));
        }

        var (prebufferSamples, playbackCapacity) = CalculatePlaybackBufferSizes(
            options.Format,
            options.TargetPlaybackBuffer);

        IRealtimeAudioSession session = new SoundFlowRealtimeAudioSession(
            runtime,
            deviceManager,
            options.Format,
            prebufferSamples,
            playbackCapacity);
        return ValueTask.FromResult(session);
    }

    internal static (int PrebufferSamples, int Capacity) CalculatePlaybackBufferSizes(
        AudioFormat format,
        TimeSpan targetPlaybackBuffer)
    {
        if (targetPlaybackBuffer <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetPlaybackBuffer),
                "The target playback buffer must be positive.");
        }

        var capacity = Math.Ceiling(
            targetPlaybackBuffer.TotalSeconds
            * format.SampleRate
            * PlaybackChannelCount);
        if (capacity > int.MaxValue / 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetPlaybackBuffer),
                "The target playback buffer is too large.");
        }

        var prebufferSamples = Math.Max(1, (int)capacity);
        return (prebufferSamples, prebufferSamples * 2);
    }
}
