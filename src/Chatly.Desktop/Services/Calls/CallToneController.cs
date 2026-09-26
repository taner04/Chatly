using Chatly.Desktop.Abstraction.Audio;
using Chatly.Desktop.Abstraction.Notification;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Calls;

[SingletonService]
public sealed partial class CallToneController(
    INotificationSoundPlayer soundPlayer,
    ILogger<CallToneController> logger) : IAsyncDisposable
{
    private IAudioPlayback? _playback;

    internal Task StartIncomingAsync() => StartAsync();

    internal Task StartOutgoingAsync() => StartAsync();

    internal async Task StopAsync()
    {
        var playback = _playback;
        _playback = null;
        if (playback is null)
        {
            return;
        }

        await CallCoordinatorUtilities.TryInvokeAsync(
            async () => await playback.StopAsync(),
            LogOperationFailed);
        try
        {
            playback.Dispose();
        }
        catch (Exception exception)
        {
            LogOperationFailed(exception, nameof(StopAsync));
        }
    }

    public async ValueTask DisposeAsync() => await StopAsync();

    private async Task StartAsync()
    {
        await StopAsync();
        try
        {
            _playback = await soundPlayer.PlayRingtoneAsync();
        }
        catch (Exception exception)
        {
            LogRingtonePlaybackFailed(exception);
        }
    }

    [LoggerMessage(LogLevel.Warning, "Call tone operation {Operation} failed.")]
    private partial void LogOperationFailed(Exception exception, string operation);

    [LoggerMessage(LogLevel.Warning, "Failed to start call ringing audio.")]
    private partial void LogRingtonePlaybackFailed(Exception exception);
}
