using System.IO;
using Chatly.Desktop.Abstraction.Audio;
using Chatly.Desktop.Services.Audio;
using SoundFlow.Components;

namespace Chatly.Desktop.Models.Audio;

internal sealed class SoundFlowAudioPlayback : IAudioPlayback, ISoundFlowRuntimeOwner
{
    private readonly Stream _audioStream;
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Lock _lifecycleLock = new();
    private readonly SoundPlayer _player;
    private readonly SoundFlowRuntime _runtime;
    private readonly Lock _syncRoot = new();
    private bool _disposed;
    private bool _isPlaying;

    public SoundFlowAudioPlayback(
        SoundFlowRuntime runtime,
        SoundPlayer player,
        Stream audioStream,
        AudioPlaybackOptions? options)
    {
        _runtime = runtime;
        _player = player;
        _audioStream = audioStream;
        _player.IsLooping = options?.Loop ?? false;
        _player.PlaybackEnded += PlayerOnPlaybackEnded;
    }

    public Task Completion => _completion.Task;

    public ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lifecycleLock)
        {
            return StopCore();
        }
    }

    public void Dispose()
    {
        lock (_lifecycleLock)
        {
            DisposeCore();
        }
    }

    void ISoundFlowRuntimeOwner.StopForRuntimeDisposal() => Dispose();

    private ValueTask StopCore()
    {
        var shouldStop = false;
        lock (_syncRoot)
        {
            if (_disposed || !_isPlaying)
            {
                return ValueTask.CompletedTask;
            }

            _isPlaying = false;
            shouldStop = true;
        }

        try
        {
            if (shouldStop)
            {
                _runtime.StopPlayback(_player);
            }
        }
        finally
        {
            _completion.TrySetResult();
        }

        return ValueTask.CompletedTask;
    }

    private void DisposeCore()
    {
        var shouldStop = false;
        lock (_syncRoot)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _player.PlaybackEnded -= PlayerOnPlaybackEnded;
            shouldStop = _isPlaying;
            _isPlaying = false;
        }

        Exception? failure = null;
        if (shouldStop)
        {
            try
            {
                lock (_lifecycleLock)
                {
                    _runtime.StopPlayback(_player);
                }
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        }

        try
        {
            _runtime.DisposePlayback(_player, _audioStream, this);
        }
        catch (Exception exception)
        {
            failure ??= exception;
        }

        _completion.TrySetResult();
        if (failure is not null)
        {
            throw failure;
        }
    }

    internal void Start()
    {
        lock (_lifecycleLock)
        {
            lock (_syncRoot)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (_isPlaying)
                {
                    return;
                }

                _isPlaying = true;
            }

            try
            {
                _runtime.StartPlayback(_player);
            }
            catch
            {
                lock (_syncRoot)
                {
                    _isPlaying = false;
                }

                throw;
            }
        }
    }

    private void PlayerOnPlaybackEnded(object? sender, EventArgs e)
    {
        lock (_syncRoot)
        {
            if (_disposed)
            {
                return;
            }

            _isPlaying = false;
        }

        _completion.TrySetResult();
    }
}