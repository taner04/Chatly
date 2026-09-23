using Chatly.Audio.Abstractions;
using Chatly.Audio.Models;
using Chatly.DependencyInjection;

namespace Chatly.Audio.Services.SoundFlow;

[SingletonService(typeof(IAudioDeviceManager))]
internal sealed class SoundFlowAudioDeviceManager(SoundFlowRuntime runtime) : IAudioDeviceManager
{
    public IReadOnlyList<AudioDeviceInfo> InputDevices { get; private set; } = [];
    public IReadOnlyList<AudioDeviceInfo> OutputDevices { get; private set; } = [];

    public AudioDeviceInfo? SelectedInputDevice { get; private set; }

    public AudioDeviceInfo? SelectedOutputDevice { get; private set; }

    public event EventHandler? Changed;

    public ValueTask RefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RefreshDevices();
        Changed?.Invoke(this, EventArgs.Empty);

        return ValueTask.CompletedTask;
    }

    public ValueTask SelectInputDeviceAsync(
        nint deviceId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var selectedInputDevice = ResolveExplicitSelection(InputDevices, deviceId);
        runtime.SwitchCaptureDevice(selectedInputDevice.Id);
        SelectedInputDevice = selectedInputDevice;

        Changed?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public ValueTask SelectOutputDeviceAsync(
        nint deviceId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var selectedOutputDevice = ResolveExplicitSelection(OutputDevices, deviceId);
        runtime.SwitchPlaybackDevice(selectedOutputDevice.Id);
        SelectedOutputDevice = selectedOutputDevice;

        Changed?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    private void RefreshDevices()
    {
        var devices = runtime.RefreshAudioDevices();
        InputDevices = devices.InputDevices;
        OutputDevices = devices.OutputDevices;
        SelectedInputDevice = ResolveSelection(InputDevices, SelectedInputDevice?.Id);
        SelectedOutputDevice = ResolveSelection(OutputDevices, SelectedOutputDevice?.Id);
    }

    private static AudioDeviceInfo? ResolveSelection(
        IReadOnlyList<AudioDeviceInfo> devices,
        nint? selectedDeviceId)
    {
        if (selectedDeviceId is { } deviceId)
        {
            var selectedDevice = devices.FirstOrDefault(device => device.Id == deviceId);
            if (selectedDevice is not null)
            {
                return selectedDevice;
            }
        }

        var defaultDevice = devices.FirstOrDefault(device => device.IsDefault);
        return defaultDevice ?? (devices.Count > 0 ? devices[0] : null);
    }

    private static AudioDeviceInfo ResolveExplicitSelection(
        IReadOnlyList<AudioDeviceInfo> devices,
        nint deviceId) =>
        devices.FirstOrDefault(device => device.Id == deviceId)
        ?? throw new ArgumentException(
            $"Audio device ID '{deviceId}' is not present in the current device snapshot.",
            nameof(deviceId));
}
