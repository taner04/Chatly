using Chatly.Desktop.Abstraction.Audio;

namespace Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;

internal sealed class FakeAudioPlayback : IAudioPlayback
{
    internal int StopCount { get; private set; }
    internal int DisposeCount { get; private set; }

    public Task Completion => Task.CompletedTask;

    public ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        StopCount++;
        return ValueTask.CompletedTask;
    }

    public void Dispose() => DisposeCount++;
}