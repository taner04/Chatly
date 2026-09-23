using Chatly.Audio.Models;

namespace Chatly.Audio.Abstractions;

public interface IAudioDeviceManager
{
    IReadOnlyList<AudioDeviceInfo> InputDevices { get; }

    IReadOnlyList<AudioDeviceInfo> OutputDevices { get; }

    AudioDeviceInfo? SelectedInputDevice { get; }

    AudioDeviceInfo? SelectedOutputDevice { get; }

    event EventHandler? Changed;

    ValueTask RefreshAsync(CancellationToken cancellationToken = default);

    ValueTask SelectInputDeviceAsync(
        nint deviceId,
        CancellationToken cancellationToken = default);

    ValueTask SelectOutputDeviceAsync(
        nint deviceId,
        CancellationToken cancellationToken = default);
}