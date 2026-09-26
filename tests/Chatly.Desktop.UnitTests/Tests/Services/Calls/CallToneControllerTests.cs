using Chatly.Desktop.Services.Calls;
using Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chatly.Desktop.UnitTests.Tests.Services.Calls;

public sealed class CallToneControllerTests
{
    [Fact]
    public async Task StartAndStop_Should_StopAndDisposePreviousPlaybackOnce_When_TonesChange()
    {
        var soundPlayer = new FakeNotificationSoundPlayer();
        var tones = new CallToneController(soundPlayer, NullLogger<CallToneController>.Instance);

        await tones.StartIncomingAsync();
        await tones.StartOutgoingAsync();

        soundPlayer.Playbacks.Should().HaveCount(2);
        soundPlayer.Playbacks[0].StopCount.Should().Be(1);
        soundPlayer.Playbacks[0].DisposeCount.Should().Be(1);

        await tones.StopAsync();
        await tones.StopAsync();

        soundPlayer.Playbacks[1].StopCount.Should().Be(1);
        soundPlayer.Playbacks[1].DisposeCount.Should().Be(1);
    }
}
