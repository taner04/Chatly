namespace Chatly.Audio.Models;

public sealed record RealtimeAudioOptions
{
    public AudioFormat Format { get; init; } = AudioFormat.Voice;

    public TimeSpan TargetPlaybackBuffer { get; init; } = TimeSpan.FromMilliseconds(80);
}
