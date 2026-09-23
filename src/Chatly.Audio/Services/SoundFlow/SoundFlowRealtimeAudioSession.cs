using System.Buffers;
using Chatly.Audio.Abstractions;
using Chatly.Audio.Enums;
using Chatly.Audio.Events;
using Chatly.Audio.Models;
using SoundFlow.Abstracts;
using SoundFlow.Enums;

namespace Chatly.Audio.Services.SoundFlow;

internal sealed class SoundFlowRealtimeAudioSession : IRealtimeAudioSession, ISoundFlowRuntimeOwner
{
    private const int CaptureQueueCapacity = 8;
    private const int MaximumCaptureSamplesPerCallback = 48_000;

    private readonly Queue<float[]> _captureQueue = new();
    private readonly Lock _captureQueueLock = new();
    private readonly CancellationTokenSource _dispatchCancellation = new();
    private readonly IAudioDeviceManager _deviceManager;
    private readonly SemaphoreSlim _dispatchSignal = new(0);
    private readonly Task _dispatchTask;
    private readonly SoundFlowRuntime _runtime;
    private readonly Lock _lifecycleLock = new();
    private readonly object _syncLock = new();
    private readonly Lock _callbackLock = new();
    private readonly int _playbackPrebufferSamples;
    private readonly int _playbackCapacity;

    private SoundFlowRealtimePlayback? _playback;
    private AudioSessionState _state = AudioSessionState.Created;
    private bool _acceptCapture;
    private bool _disposed;
    private int _dispatchDisposed;
    private int _pendingCallbacks;
    private Task _callbackTail = Task.CompletedTask;

    internal SoundFlowRealtimeAudioSession(
        SoundFlowRuntime runtime,
        IAudioDeviceManager deviceManager,
        AudioFormat format,
        int playbackPrebufferSamples,
        int playbackCapacity)
    {
        _runtime = runtime;
        _deviceManager = deviceManager;
        _playbackPrebufferSamples = playbackPrebufferSamples;
        _playbackCapacity = playbackCapacity;
        Format = format;
        _runtime.RegisterOwner(this);
        _dispatchTask = DispatchCapturedAudioAsync();
    }

    public AudioFormat Format { get; }

    public AudioSessionState State
    {
        get
        {
            lock (_syncLock)
            {
                return _state;
            }
        }
    }

    public event EventHandler<AudioSessionFaultedEventArgs>? Faulted;

    public event Action<ReadOnlyMemory<float>>? AudioCaptured;

    public ValueTask StartAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lifecycleLock)
        {
            return StartCore();
        }
    }

    private ValueTask StartCore()
    {
        Exception? failure = null;

        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_state == AudioSessionState.Running)
            {
                return ValueTask.CompletedTask;
            }

            SoundFlowRealtimePlayback? playback = null;
            var playbackStarted = false;
            var captureStarted = false;

            try
            {
                playback = _runtime.CreateRealtimePlayback(
                    _deviceManager.SelectedOutputDevice?.Id,
                    _playbackPrebufferSamples,
                    _playbackCapacity);
                _runtime.StartRealtimePlayback(playback);
                playbackStarted = true;

                _runtime.StartCapture(
                    _deviceManager.SelectedInputDevice?.Id,
                    OnAudioProcessed);
                captureStarted = true;

                lock (_captureQueueLock)
                {
                    _acceptCapture = true;
                }

                _playback = playback;
                _state = AudioSessionState.Running;
            }
            catch (Exception exception)
            {
                failure = exception;
                lock (_captureQueueLock)
                {
                    _acceptCapture = false;
                    _captureQueue.Clear();
                }

                if (captureStarted)
                {
                    TryCleanup(() => _runtime.StopCapture(OnAudioProcessed));
                }

                if (playback is not null)
                {
                    if (playbackStarted)
                    {
                        TryCleanup(() => _runtime.StopRealtimePlayback(playback));
                    }

                    TryCleanup(() => _runtime.DisposeRealtimePlayback(playback));
                }

                _playback = null;
                _state = AudioSessionState.Faulted;
            }
        }

        if (failure is not null)
        {
            RaiseFaulted(failure);
            throw failure;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask StopAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var failure = StopCore();
        if (failure is not null)
        {
            RaiseFaulted(failure);
            throw failure;
        }

        return ValueTask.CompletedTask;
    }

    public bool TryWritePlayback(
        ReadOnlySpan<float> interleavedSamples)
    {
        if (!Monitor.TryEnter(_syncLock))
        {
            return false;
        }

        try
        {
            if (_disposed || _state != AudioSessionState.Running || _playback is null)
            {
                return false;
            }

            if (interleavedSamples.IsEmpty)
            {
                return true;
            }

            if (interleavedSamples.Length > int.MaxValue / 2)
            {
                return false;
            }

            var stereoSampleCount = interleavedSamples.Length * 2;
            if (stereoSampleCount > _playback.Capacity - _playback.Provider.SamplesAvailable)
            {
                return false;
            }

            var stereoSamples = ArrayPool<float>.Shared.Rent(stereoSampleCount);
            try
            {
                for (var sourceIndex = 0; sourceIndex < interleavedSamples.Length; sourceIndex++)
                {
                    var sample = interleavedSamples[sourceIndex];
                    var targetIndex = sourceIndex * 2;
                    stereoSamples[targetIndex] = sample;
                    stereoSamples[targetIndex + 1] = sample;
                }

                try
                {
                    _playback.Provider.AddSamples(
                        stereoSamples.AsSpan(0, stereoSampleCount));
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
            finally
            {
                ArrayPool<float>.Shared.Return(stereoSamples);
            }
        }
        finally
        {
            Monitor.Exit(_syncLock);
        }
    }

    public void ClearPlayback()
    {
        lock (_syncLock)
        {
            if (!_disposed)
            {
                _playback?.Provider.Reset();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        var failure = DisposeCore();
        await _dispatchTask.ConfigureAwait(false);
        DisposeDispatchResources();

        if (failure is not null)
        {
            throw failure;
        }
    }

    private Exception? StopCore()
    {
        lock (_lifecycleLock)
        {
            return StopCoreLocked();
        }
    }

    private Exception? StopCoreLocked()
    {
        SoundFlowRealtimePlayback? playback;
        var stopCapture = false;
        lock (_syncLock)
        {
            if (_disposed || _state == AudioSessionState.Stopped)
            {
                return null;
            }

            (playback, stopCapture) = DetachAudioResources();
        }

        var failure = ReleaseAudioResources(playback, stopCapture);
        lock (_syncLock)
        {
            if (_disposed)
            {
                return failure;
            }

            _state = failure is null
                ? AudioSessionState.Stopped
                : AudioSessionState.Faulted;
            return failure;
        }
    }

    private (SoundFlowRealtimePlayback? Playback, bool StopCapture) DetachAudioResources()
    {
        lock (_captureQueueLock)
        {
            _acceptCapture = false;
            _captureQueue.Clear();
        }

        var stopCapture = _state == AudioSessionState.Running;
        var playback = _playback;
        _playback = null;
        return (playback, stopCapture);
    }

    private Exception? ReleaseAudioResources(
        SoundFlowRealtimePlayback? playback,
        bool stopCapture)
    {
        Exception? failure = null;
        if (stopCapture)
        {
            try
            {
                _runtime.StopCapture(OnAudioProcessed);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        }

        if (playback is not null)
        {
            try
            {
                _runtime.StopRealtimePlayback(playback);
            }
            catch (Exception exception)
            {
                failure ??= exception;
            }

            try
            {
                _runtime.DisposeRealtimePlayback(playback);
            }
            catch (Exception exception)
            {
                failure ??= exception;
            }
        }

        return failure;
    }

    private void OnAudioProcessed(
        Span<float> samples,
        Capability capability)
    {
        if (capability != Capability.Record || samples.IsEmpty)
        {
            return;
        }

        lock (_captureQueueLock)
        {
            if (!_acceptCapture
                || _captureQueue.Count >= CaptureQueueCapacity
                || samples.Length > MaximumCaptureSamplesPerCallback)
            {
                return;
            }

            _captureQueue.Enqueue(samples.ToArray());
        }

        _dispatchSignal.Release();
    }

    private async Task DispatchCapturedAudioAsync()
    {
        while (true)
        {
            try
            {
                await _dispatchSignal.WaitAsync(_dispatchCancellation.Token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            float[]? samples;
            lock (_captureQueueLock)
            {
                samples = _captureQueue.TryDequeue(out var captured)
                    ? captured
                    : null;
            }

            if (samples is null)
            {
                continue;
            }

            lock (_callbackLock)
            {
                if (Volatile.Read(ref _pendingCallbacks) >= CaptureQueueCapacity)
                {
                    continue;
                }

                Interlocked.Increment(ref _pendingCallbacks);
                _callbackTail = _callbackTail.ContinueWith(
                    _ =>
                    {
                        try
                        {
                            PublishCapturedAudio(samples);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _pendingCallbacks);
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default);
            }
        }
    }

    private void PublishCapturedAudio(float[] samples)
    {
        var subscribers = AudioCaptured?.GetInvocationList();
        if (subscribers is null)
        {
            return;
        }

        foreach (var subscriber in subscribers)
        {
            try
            {
                ((Action<ReadOnlyMemory<float>>)subscriber)(samples);
            }
            catch
            {
                // One consumer must not interrupt delivery to the remaining consumers.
            }
        }
    }

    private Exception? DisposeCore()
    {
        lock (_lifecycleLock)
        {
            return DisposeCoreLocked();
        }
    }

    private Exception? DisposeCoreLocked()
    {
        SoundFlowRealtimePlayback? playback;
        bool stopCapture;
        lock (_syncLock)
        {
            if (_disposed)
            {
                return null;
            }

            (playback, stopCapture) = DetachAudioResources();
            _disposed = true;
            _state = AudioSessionState.Disposed;
        }

        _runtime.UnregisterOwner(this);
        var failure = ReleaseAudioResources(playback, stopCapture);
        _dispatchCancellation.Cancel();
        _dispatchSignal.Release();
        return failure;
    }

    void ISoundFlowRuntimeOwner.StopForRuntimeDisposal()
    {
        DisposeCore();
        _dispatchTask.GetAwaiter().GetResult();
        DisposeDispatchResources();
    }

    private void DisposeDispatchResources()
    {
        if (Interlocked.Exchange(ref _dispatchDisposed, 1) != 0)
        {
            return;
        }

        _dispatchSignal.Dispose();
        _dispatchCancellation.Dispose();
    }

    private void RaiseFaulted(Exception exception)
    {
        var subscribers = Faulted?.GetInvocationList();
        if (subscribers is null)
        {
            return;
        }

        var eventArgs = new AudioSessionFaultedEventArgs(exception);
        foreach (var subscriber in subscribers)
        {
            try
            {
                ((EventHandler<AudioSessionFaultedEventArgs>)subscriber)(this, eventArgs);
            }
            catch
            {
                // A fault observer must not take down capture dispatch or hide the original fault.
            }
        }
    }

    private static void TryCleanup(Action cleanup)
    {
        try
        {
            cleanup();
        }
        catch
        {
            // Preserve the exception that caused transactional startup to fail.
        }
    }
}
