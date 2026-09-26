using Avalonia.Platform;
using Chatly.Desktop.Abstraction.Audio;
using Chatly.Desktop.Models.Audio;
using Chatly.Desktop.Abstraction.Notification;
using Chatly.Desktop.Models.Settings;

namespace Chatly.Desktop.Services.NotificationService;

[SingletonService(typeof(INotificationSoundPlayer))]
internal sealed partial class CrossPlatformNotificationSoundPlayer(
    AppSettings appSettings,
    IAudioHost audioHost,
    ILogger<CrossPlatformNotificationSoundPlayer> logger)
    : INotificationSoundPlayer
{
    public async Task PlayNotificationAsync()
    {
        if (!appSettings.NotificationSettings.PlaySound)
        {
            return;
        }

        try
        {
            await using var audio = AssetLoader.Open(new Uri("avares://Chatly.Desktop/Assets/notification.wav"));

            using var playback = await audioHost.AssetPlayer.StartAsync(
                audio,
                new AudioPlaybackOptions(),
                CancellationToken.None);

            await playback.Completion;
        }
        catch (Exception exception)
        {
            LogPlaybackFailed(exception);
        }
    }

    public async Task<IAudioPlayback> PlayRingtoneAsync()
    {
        try
        {
            await using var audio = AssetLoader.Open(new Uri("avares://Chatly.Desktop/Assets/ringtone.wav"));

            return await audioHost.AssetPlayer.StartAsync(
                audio,
                new AudioPlaybackOptions { Loop = true },
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            LogRingtonePlaybackFailed(exception);
            throw;
        }
    }

    [LoggerMessage(
        LogLevel.Warning,
        "Failed to play the notification sound.")]
    private partial void LogPlaybackFailed(Exception exception);

    [LoggerMessage(
        LogLevel.Warning,
        "Failed to play the ringtone.")]
    private partial void LogRingtonePlaybackFailed(Exception exception);
}