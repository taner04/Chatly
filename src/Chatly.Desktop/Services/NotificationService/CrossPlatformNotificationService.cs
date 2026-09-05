using System.Linq;
using Avalonia.Platform;
using Chatly.Desktop.Abstraction.Notification;
using Chatly.Desktop.Models.Settings;
using Microsoft.Extensions.Logging;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace Chatly.Desktop.Services.NotificationService;

[SingletonService(typeof(INotificationService))]
internal sealed partial class CrossPlatformNotificationService(
    AppSettings appSettings,
    ILogger<CrossPlatformNotificationService> logger) : INotificationService, IDisposable
{
    private static readonly Uri NotificationUri = new("avares://Chatly.Desktop/Assets/notification.wav");

    private static readonly AudioFormat Format = AudioFormat.DvdHq;

    private readonly Lock _syncLock = new();
    private bool _disposed;

    private MiniAudioEngine? _engine;
    private AudioPlaybackDevice? _playbackDevice;
    private SoundPlayer? _player;
    private AssetDataProvider? _provider;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        lock (_syncLock)
        {
            DisposeAudioResources();
        }
    }

    public Task PlayNotificationSoundAsync()
    {
        if (!appSettings.NotificationSettings.PlaySound || _disposed)
        {
            return Task.CompletedTask;
        }

        try
        {
            lock (_syncLock)
            {
                if (!EnsureInitialized())
                {
                    return Task.CompletedTask;
                }

                EnsureDefaultPlaybackDevice();
                _player!.Stop();
                _player.Play();
            }
        }
        catch (Exception exception)
        {
            LogPlaybackFailed(exception);
        }

        return Task.CompletedTask;
    }

    private bool EnsureInitialized()
    {
        if (_player is not null)
        {
            return true;
        }

        try
        {
            _engine = new MiniAudioEngine();
            var defaultDevice = GetDefaultPlaybackDevice();
            if (defaultDevice is null)
            {
                DisposeAudioResources();
                return false;
            }

            _playbackDevice = _engine.InitializePlaybackDevice(defaultDevice.Value, Format);
            using var stream = AssetLoader.Open(NotificationUri);
            _provider = new AssetDataProvider(_engine, Format, stream);
            _player = new SoundPlayer(_engine, Format, _provider);
            _playbackDevice.MasterMixer.AddComponent(_player);
            _playbackDevice.Start();
            return true;
        }
        catch
        {
            DisposeAudioResources();
            throw;
        }
    }

    private void EnsureDefaultPlaybackDevice()
    {
        var defaultDevice = GetDefaultPlaybackDevice();
        if (defaultDevice is null ||
            _playbackDevice is null ||
            _playbackDevice.Info?.Id == defaultDevice.Value.Id)
        {
            return;
        }

        _playbackDevice = _engine!.SwitchDevice(_playbackDevice, defaultDevice.Value);
    }

    private DeviceInfo? GetDefaultPlaybackDevice()
    {
        _engine!.UpdateAudioDevicesInfo();

        return _engine.PlaybackDevices.FirstOrDefault(x => x.IsDefault);
    }

    private void DisposeAudioResources()
    {
        _player?.Dispose();
        _provider?.Dispose();
        _playbackDevice?.Dispose();
        _engine?.Dispose();

        _player = null;
        _provider = null;
        _playbackDevice = null;
        _engine = null;
    }

    [LoggerMessage(LogLevel.Warning, "Failed to play the notification sound.")]
    private partial void LogPlaybackFailed(Exception exception);
}