using Chatly.Audio.Abstractions;

namespace Chatly.Desktop.Abstraction.Notification;

public interface INotificationSoundPlayer
{
    Task PlayNotificationAsync();
    Task<IAudioPlayback> PlayRingtoneAsync();
}
