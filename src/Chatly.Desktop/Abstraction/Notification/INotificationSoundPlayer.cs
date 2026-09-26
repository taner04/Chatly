using Chatly.Desktop.Abstraction.Audio;

namespace Chatly.Desktop.Abstraction.Notification;

public interface INotificationSoundPlayer
{
    Task PlayNotificationAsync();
    Task<IAudioPlayback> PlayRingtoneAsync();
}