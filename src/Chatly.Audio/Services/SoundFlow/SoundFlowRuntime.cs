using Chatly.Audio.Models;
using Chatly.DependencyInjection;
using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;
using AudioFormat = SoundFlow.Structs.AudioFormat;

namespace Chatly.Audio.Services.SoundFlow;

[SingletonService]
internal sealed class SoundFlowRuntime : IDisposable
{
    private static readonly AudioFormat PlaybackFormat = new()
    {
        Format = SampleFormat.F32,
        Channels = 2,
        Layout = AudioFormat.GetLayoutFromChannels(2),
        SampleRate = 48000
    };

    private static readonly AudioFormat CaptureFormat = new()
    {
        Format = SampleFormat.F32,
        Channels = 1,
        Layout = AudioFormat.GetLayoutFromChannels(1),
        SampleRate = 48000
    };

    private readonly Lock _syncLock = new();
    private readonly HashSet<AudioProcessCallback> _captureCallbacks = [];
    private readonly HashSet<SoundPlayer> _mixerComponents = [];
    private readonly HashSet<ISoundFlowRuntimeOwner> _owners = [];

    private bool _disposed;

    private AudioPlaybackDevice? _playbackDevice;
    private AudioCaptureDevice? _captureDevice;
    private DeviceInfo[] _playbackDevices = [];
    private DeviceInfo[] _captureDevices = [];

    private MiniAudioEngine Engine { get; } = new();

    public void Dispose()
    {
        ISoundFlowRuntimeOwner[] owners;
        lock (_syncLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            owners = [.. _owners];
        }

        Exception? failure = null;
        foreach (var owner in owners)
        {
            try
            {
                owner.StopForRuntimeDisposal();
            }
            catch (Exception exception)
            {
                failure ??= exception;
            }
        }

        AudioCaptureDevice? captureDevice;
        AudioPlaybackDevice? playbackDevice;
        SoundPlayer[] components;
        lock (_syncLock)
        {
            captureDevice = _captureDevice;
            playbackDevice = _playbackDevice;
            components = [.. _mixerComponents];
            _captureDevice = null;
            _playbackDevice = null;
            _captureCallbacks.Clear();
            _mixerComponents.Clear();
            _owners.Clear();
        }

        try
        {
            captureDevice?.Dispose();
        }
        catch (Exception exception)
        {
            failure ??= exception;
        }

        if (playbackDevice is not null)
        {
            foreach (var component in components)
            {
                try
                {
                    playbackDevice.MasterMixer.RemoveComponent(component);
                    component.Dispose();
                }
                catch (Exception exception)
                {
                    failure ??= exception;
                }
            }
        }

        try
        {
            playbackDevice?.Dispose();
        }
        catch (Exception exception)
        {
            failure ??= exception;
        }

        try
        {
            Engine.Dispose();
        }
        catch (Exception exception)
        {
            failure ??= exception;
        }

        if (failure is not null)
        {
            throw failure;
        }
    }

    internal SoundFlowAudioPlayback CreatePlayback(
        Stream audioStream,
        nint? selectedDeviceId,
        AudioPlaybackOptions? options)
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            EnsurePlaybackDevice(selectedDeviceId);

            AssetDataProvider? provider = null;
            SoundPlayer? player = null;

            try
            {
                provider = new AssetDataProvider(
                    Engine,
                    PlaybackFormat,
                    audioStream);

                player = new SoundPlayer(
                    Engine,
                    PlaybackFormat,
                    provider);

                var playback = new SoundFlowAudioPlayback(
                    this,
                    player,
                    audioStream,
                    options);
                _owners.Add(playback);
                return playback;
            }
            catch
            {
                if (player is not null)
                {
                    player.Dispose();
                }
                else
                {
                    provider?.Dispose();
                }

                throw;
            }
        }
    }

    internal SoundFlowRealtimePlayback CreateRealtimePlayback(
        nint? selectedDeviceId,
        int prebufferSamples,
        int capacity)
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            EnsurePlaybackDevice(selectedDeviceId);

            var playbackDevice = _playbackDevice
                ?? throw new InvalidOperationException(
                    "The playback device has not been initialized.");

            BufferedQueueDataProvider? provider = null;
            SoundPlayer? player = null;

            try
            {
                provider = new BufferedQueueDataProvider(
                    PlaybackFormat,
                    prebufferSamples,
                    capacity);

                player = new SoundPlayer(
                    Engine,
                    PlaybackFormat,
                    provider);

                playbackDevice.MasterMixer.AddComponent(player);
                _mixerComponents.Add(player);

                return new SoundFlowRealtimePlayback(
                    player,
                    provider,
                    capacity);
            }
            catch
            {
                if (player is not null)
                {
                    if (_playbackDevice is not null)
                    {
                        _playbackDevice.MasterMixer.RemoveComponent(player);
                    }

                    _mixerComponents.Remove(player);
                    player.Dispose();
                }
                else
                {
                    provider?.Dispose();
                }

                throw;
            }
        }
    }

    internal (
        AudioDeviceInfo[] InputDevices,
        AudioDeviceInfo[] OutputDevices)
        RefreshAudioDevices()
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            Engine.UpdateAudioDevicesInfo();
            _captureDevices = [.. Engine.CaptureDevices];
            _playbackDevices = [.. Engine.PlaybackDevices];

            var inputDevices = _captureDevices
                .Select(device => new AudioDeviceInfo(
                    device.Id,
                    device.Name,
                    device.IsDefault))
                .ToArray();

            var outputDevices = _playbackDevices
                .Select(device => new AudioDeviceInfo(
                    device.Id,
                    device.Name,
                    device.IsDefault))
                .ToArray();

            return (
                inputDevices,
                outputDevices);
        }
    }

    internal void StartPlayback(
        SoundPlayer player)
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var playbackDevice = _playbackDevice
                ?? throw new InvalidOperationException(
                    "The playback device has not been initialized.");

            playbackDevice.MasterMixer.AddComponent(player);
            _mixerComponents.Add(player);

            try
            {
                player.Play();
            }
            catch
            {
                playbackDevice.MasterMixer.RemoveComponent(player);
                _mixerComponents.Remove(player);

                throw;
            }
        }
    }

    internal void StartRealtimePlayback(
        SoundFlowRealtimePlayback playback)
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            playback.Player.Play();
        }
    }

    internal void StartCapture(
        nint? selectedDeviceId,
        AudioProcessCallback callback)
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            EnsureCaptureDevice(selectedDeviceId);
            var captureDevice = _captureDevice
                ?? throw new InvalidOperationException(
                    "The capture device has not been initialized.");

            if (!_captureCallbacks.Add(callback))
            {
                return;
            }

            captureDevice.OnAudioProcessed += callback;
            try
            {
                if (_captureCallbacks.Count == 1)
                {
                    captureDevice.Start();
                }
            }
            catch
            {
                captureDevice.OnAudioProcessed -= callback;
                _captureCallbacks.Remove(callback);
                throw;
            }
        }
    }

    internal void StopPlayback(
        SoundPlayer player)
    {
        player.Stop();
    }

    internal void StopRealtimePlayback(
        SoundFlowRealtimePlayback playback)
    {
        playback.Player.Stop();
        playback.Provider.Reset();
    }

    internal void StopCapture(
        AudioProcessCallback callback)
    {
        AudioCaptureDevice? captureDevice;
        var stopDevice = false;
        lock (_syncLock)
        {
            captureDevice = _captureDevice;
            if (captureDevice is null)
            {
                return;
            }

            if (!_captureCallbacks.Remove(callback))
            {
                return;
            }

            captureDevice.OnAudioProcessed -= callback;
            stopDevice = _captureCallbacks.Count == 0;
        }

        if (stopDevice)
        {
            captureDevice.Stop();
        }
    }

    internal void DisposePlayback(
        SoundPlayer player,
        Stream audio,
        ISoundFlowRuntimeOwner owner)
    {
        AudioPlaybackDevice? playbackDevice;
        lock (_syncLock)
        {
            playbackDevice = _playbackDevice;
            _mixerComponents.Remove(player);
            _owners.Remove(owner);
        }

        playbackDevice?.MasterMixer.RemoveComponent(player);
        player.Dispose();
        audio.Dispose();
    }

    internal void DisposeRealtimePlayback(
        SoundFlowRealtimePlayback playback)
    {
        AudioPlaybackDevice? playbackDevice;
        lock (_syncLock)
        {
            playbackDevice = _playbackDevice;
            _mixerComponents.Remove(playback.Player);
        }

        playbackDevice?.MasterMixer.RemoveComponent(playback.Player);
        playback.Player.Dispose();
    }

    internal void RegisterOwner(ISoundFlowRuntimeOwner owner)
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _owners.Add(owner);
        }
    }

    internal void UnregisterOwner(ISoundFlowRuntimeOwner owner)
    {
        lock (_syncLock)
        {
            _owners.Remove(owner);
        }
    }

    internal void SwitchPlaybackDevice(
        nint deviceId)
    {
        AudioPlaybackDevice? oldDevice = null;
        AudioPlaybackDevice? replacement = null;
        SoundPlayer[] components = [];
        Exception? failure = null;
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_playbackDevice is null)
            {
                return;
            }

            var selectedDevice =
                ResolvePlaybackDevice(deviceId);

            if (_playbackDevice.Info?.Id ==
                selectedDevice.Id)
            {
                return;
            }

            try
            {
                replacement = Engine.InitializePlaybackDevice(selectedDevice, PlaybackFormat);
                components = [.. _mixerComponents];
                foreach (var component in components)
                {
                    replacement.MasterMixer.AddComponent(component);
                }

                replacement.Start();
                oldDevice = _playbackDevice;
                _playbackDevice = replacement;
                replacement = null;
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        }

        var detachedDevice = oldDevice ?? replacement;
        if (detachedDevice is not null)
        {
            foreach (var component in components)
            {
                TryCleanup(() => detachedDevice.MasterMixer.RemoveComponent(component));
            }
        }

        if (replacement is not null)
        {
            TryCleanup(replacement.Dispose);
        }

        if (oldDevice is not null)
        {
            TryCleanup(oldDevice.Dispose);
        }

        if (failure is not null)
        {
            throw failure;
        }
    }

    internal void SwitchCaptureDevice(
        nint deviceId)
    {
        AudioCaptureDevice? oldDevice = null;
        AudioCaptureDevice? replacement = null;
        AudioProcessCallback[] callbacks = [];
        Exception? failure = null;
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_captureDevice is null)
            {
                return;
            }

            var selectedDevice =
                ResolveCaptureDevice(deviceId);

            if (_captureDevice.Info?.Id ==
                selectedDevice.Id)
            {
                return;
            }

            try
            {
                replacement = Engine.InitializeCaptureDevice(selectedDevice, CaptureFormat);
                callbacks = [.. _captureCallbacks];
                foreach (var callback in callbacks)
                {
                    replacement.OnAudioProcessed += callback;
                }

                if (_captureCallbacks.Count > 0)
                {
                    replacement.Start();
                }

                oldDevice = _captureDevice;
                _captureDevice = replacement;
                replacement = null;
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        }

        var detachedDevice = oldDevice ?? replacement;
        if (detachedDevice is not null)
        {
            foreach (var callback in callbacks)
            {
                TryCleanup(() => detachedDevice.OnAudioProcessed -= callback);
            }
        }

        if (replacement is not null)
        {
            TryCleanup(replacement.Dispose);
        }

        if (oldDevice is not null)
        {
            TryCleanup(oldDevice.Dispose);
        }

        if (failure is not null)
        {
            throw failure;
        }
    }

    private void EnsurePlaybackDevice(
        nint? selectedDeviceId)
    {
        var selectedDevice =
            ResolvePlaybackDevice(
                selectedDeviceId);

        if (_playbackDevice?.Info?.Id ==
            selectedDevice.Id)
        {
            return;
        }

        if (_playbackDevice is not null)
        {
            throw new InvalidOperationException("Select the playback device before starting playback.");
        }

        _playbackDevice =
            Engine.InitializePlaybackDevice(
                selectedDevice,
                PlaybackFormat);

        _playbackDevice.Start();
    }

    private void EnsureCaptureDevice(
        nint? selectedDeviceId)
    {
        var selectedDevice =
            ResolveCaptureDevice(
                selectedDeviceId);

        if (_captureDevice?.Info?.Id ==
            selectedDevice.Id)
        {
            return;
        }

        if (_captureDevice is not null)
        {
            throw new InvalidOperationException("Select the capture device before starting capture.");
        }

        _captureDevice =
            Engine.InitializeCaptureDevice(
                selectedDevice,
                CaptureFormat);
    }

    private DeviceInfo ResolvePlaybackDevice(
        nint? selectedDeviceId)
    {
        return ResolveDevice(
            _playbackDevices,
            selectedDeviceId);
    }

    private DeviceInfo ResolveCaptureDevice(
        nint? selectedDeviceId)
    {
        return ResolveDevice(
            _captureDevices,
            selectedDeviceId);
    }

    private static DeviceInfo ResolveDevice(
        DeviceInfo[] devices,
        nint? selectedDeviceId)
    {
        if (devices.Length == 0)
        {
            throw new InvalidOperationException(
                "No audio device is available.");
        }

        if (selectedDeviceId is { } deviceId)
        {
            var selectedIndex = Array.FindIndex(
                devices,
                device => device.Id == deviceId);

            if (selectedIndex >= 0)
            {
                return devices[selectedIndex];
            }

            throw new ArgumentException(
                $"Audio device ID '{deviceId}' is not present in the current device snapshot.",
                nameof(selectedDeviceId));
        }

        var defaultIndex = Array.FindIndex(
            devices,
            device => device.IsDefault);

        return defaultIndex >= 0
            ? devices[defaultIndex]
            : devices[0];
    }

    private static void TryCleanup(Action cleanup)
    {
        try
        {
            cleanup();
        }
        catch
        {
            // Device replacement has already either committed or failed; cleanup cannot change that result.
        }
    }
}

internal interface ISoundFlowRuntimeOwner
{
    void StopForRuntimeDisposal();
}
