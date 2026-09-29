using System.IO;
using Chatly.Desktop.Models.Audio;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace Chatly.Desktop.Services.Audio;

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

    private readonly HashSet<SoundPlayer> _mixerComponents = [];
    private readonly HashSet<ISoundFlowRuntimeOwner> _owners = [];

    private readonly Lock _syncLock = new();

    private bool _disposed;

    private AudioPlaybackDevice? _playbackDevice;
    private DeviceInfo[] _playbackDevices = [];

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

        AudioPlaybackDevice? playbackDevice;
        SoundPlayer[] components;
        lock (_syncLock)
        {
            playbackDevice = _playbackDevice;
            components = [.. _mixerComponents];
            _playbackDevice = null;
            _mixerComponents.Clear();
            _owners.Clear();
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
        AudioPlaybackOptions? options)
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            EnsurePlaybackDevice();

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

    internal void RefreshAudioDevices()
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            Engine.UpdateAudioDevicesInfo();
            _playbackDevices = [.. Engine.PlaybackDevices];
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

    internal void StopPlayback(
        SoundPlayer player)
    {
        player.Stop();
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

    private void EnsurePlaybackDevice()
    {
        if (_playbackDevice is not null)
        {
            return;
        }

        if (_playbackDevices.Length == 0)
        {
            throw new InvalidOperationException("No audio device is available.");
        }

        var defaultIndex = Array.FindIndex(_playbackDevices, device => device.IsDefault);
        _playbackDevice = Engine.InitializePlaybackDevice(
            _playbackDevices[Math.Max(defaultIndex, 0)],
            PlaybackFormat);
        _playbackDevice.Start();
    }
}

internal interface ISoundFlowRuntimeOwner
{
    void StopForRuntimeDisposal();
}