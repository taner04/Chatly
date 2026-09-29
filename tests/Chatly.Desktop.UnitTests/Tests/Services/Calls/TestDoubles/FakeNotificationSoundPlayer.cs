using Chatly.Desktop.Abstraction.Audio;
using Chatly.Desktop.Abstraction.Notification;

namespace Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;

internal sealed class FakeNotificationSoundPlayer : INotificationSoundPlayer
{
    internal List<FakeAudioPlayback> Playbacks { get; } = [];

    internal int NotificationCount { get; private set; }

    public Task PlayNotificationAsync()
    {
        NotificationCount++;
        return Task.CompletedTask;
    }

    public Task<IAudioPlayback> PlayRingtoneAsync()
    {
        var playback = new FakeAudioPlayback();
        Playbacks.Add(playback);
        return Task.FromResult<IAudioPlayback>(playback);
    }
}