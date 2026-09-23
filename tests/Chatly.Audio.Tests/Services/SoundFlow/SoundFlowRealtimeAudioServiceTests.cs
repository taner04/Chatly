using Chatly.Audio.Abstractions;
using Chatly.Audio.Enums;
using Chatly.Audio.Models;
using Chatly.Audio.Services.SoundFlow;
using NSubstitute;

namespace Chatly.Audio.Tests.Services.SoundFlow;

public sealed class SoundFlowRealtimeAudioServiceTests
{
    [Fact]
    public async Task CreateSessionRejectsNullOptions()
    {
        using var runtime = new SoundFlowRuntime();
        var service = CreateService(runtime);

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.CreateSessionAsync(null!));
    }

    [Fact]
    public async Task CreateSessionHonorsPreCanceledToken()
    {
        using var runtime = new SoundFlowRuntime();
        var service = CreateService(runtime);

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await service.CreateSessionAsync(
                new RealtimeAudioOptions(),
                new CancellationToken(canceled: true)));
    }

    [Fact]
    public async Task CreateSessionRejectsNonVoiceFormat()
    {
        using var runtime = new SoundFlowRuntime();
        var service = CreateService(runtime);
        var options = new RealtimeAudioOptions
        {
            Format = new AudioFormat(44_100, 1)
        };

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.CreateSessionAsync(options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateSessionRejectsNonPositiveTargetBuffer(double milliseconds)
    {
        using var runtime = new SoundFlowRuntime();
        var service = CreateService(runtime);
        var options = new RealtimeAudioOptions
        {
            TargetPlaybackBuffer = TimeSpan.FromMilliseconds(milliseconds)
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await service.CreateSessionAsync(options));
    }

    [Fact]
    public async Task CreateSessionRejectsTargetBufferBeyondIntegerCapacity()
    {
        using var runtime = new SoundFlowRuntime();
        var service = CreateService(runtime);
        var options = new RealtimeAudioOptions
        {
            TargetPlaybackBuffer = TimeSpan.MaxValue
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await service.CreateSessionAsync(options));
    }

    [Fact]
    public void VoiceBufferSizesAccountForStereoPlaybackAndBurstCapacity()
    {
        var sizes = SoundFlowRealtimeAudioService.CalculatePlaybackBufferSizes(
            AudioFormat.Voice,
            TimeSpan.FromMilliseconds(60));

        Assert.Equal(5_760, sizes.PrebufferSamples);
        Assert.Equal(11_520, sizes.Capacity);
    }

    [Fact]
    public async Task CreateSessionReturnsIndependentUnstartedSessions()
    {
        using var runtime = new SoundFlowRuntime();
        var service = CreateService(runtime);
        var first = await service.CreateSessionAsync(new RealtimeAudioOptions());
        var second = await service.CreateSessionAsync(new RealtimeAudioOptions());

        try
        {
            Assert.NotSame(first, second);
            Assert.Equal(AudioFormat.Voice, first.Format);
            Assert.Equal(AudioFormat.Voice, second.Format);
            Assert.Equal(AudioSessionState.Created, first.State);
            Assert.Equal(AudioSessionState.Created, second.State);
            Assert.False(first.TryWritePlayback([0.25f]));
            Assert.False(second.TryWritePlayback([0.25f]));
        }
        finally
        {
            await first.DisposeAsync();
            await second.DisposeAsync();
        }
    }

    [Fact]
    public async Task UnstartedSessionSupportsHardwareFreeLifecycleOperations()
    {
        using var runtime = new SoundFlowRuntime();
        var service = CreateService(runtime);
        var session = await service.CreateSessionAsync(new RealtimeAudioOptions());

        session.ClearPlayback();
        await session.StopAsync();
        Assert.Equal(AudioSessionState.Stopped, session.State);

        await session.DisposeAsync();
        await session.DisposeAsync();
        Assert.Equal(AudioSessionState.Disposed, session.State);
    }

    private static SoundFlowRealtimeAudioService CreateService(SoundFlowRuntime runtime) =>
        new(runtime, Substitute.For<IAudioDeviceManager>());
}
