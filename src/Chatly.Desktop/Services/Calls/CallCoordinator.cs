using Chatly.Audio.Abstractions;
using Chatly.Audio.Events;
using Chatly.Audio.Models;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.Desktop.Abstraction.Notification;
using Chatly.Rtc.Abstractions;
using Chatly.Rtc.Enums;
using Chatly.Rtc.Events;
using RtcIceCandidate = Chatly.Rtc.Models.IceCandidate;

namespace Chatly.Desktop.Services.Calls;

[SingletonService]
public sealed partial class CallCoordinator(
    ICallingHubServer callingHub,
    IWebRtcPeerFactory peerFactory,
    IAudioHost audioHost,
    INotificationSoundPlayer soundPlayer,
    ILogger<CallCoordinator> logger) : IAsyncDisposable
{
    private readonly SemaphoreSlim _operations = new(1, 1);
    private readonly Lock _lifecycleLock = new();
    private readonly HashSet<Task> _callbackTasks = [];
    private CallSnapshot _snapshot = CallSnapshot.Empty;
    private CallRole? _role;
    private IWebRtcPeer? _peer;
    private IRealtimeAudioSession? _audioSession;
    private IAudioPlayback? _ringtone;
    private IAudioPlayback? _ringback;
    private Task? _shutdownTask;
    private int _stopping;
    private bool _disposed;

    public CallSnapshot Snapshot => Volatile.Read(ref _snapshot);

    public event Action<CallSnapshot>? SnapshotChanged;

    public Task StartCallAsync(Guid remoteUserId, CancellationToken cancellationToken = default) =>
        RunWhileRunningAsync(async () =>
        {
            ThrowIfDisposed();
            if (_snapshot.HasCall)
            {
                return;
            }

            var call = await callingHub.StartCallAsync(remoteUserId);
            SetCall(call);
            _ringback = await TryStartRingtoneAsync();
        }, cancellationToken);

    public Task AcceptAsync(CancellationToken cancellationToken = default) =>
        RunWhileRunningAsync(async () =>
        {
            if (_snapshot is not { CallId: { } callId, IsIncoming: true })
            {
                return;
            }

            await callingHub.AcceptCallAsync(callId);
            await StopRingtonesAsync();
            SetState(CallState.Accepted);
            await EnsureMediaAsync(cancellationToken);
        }, cancellationToken);

    public Task RejectAsync(CancellationToken cancellationToken = default) =>
        RunWhileRunningAsync(async () =>
        {
            if (_snapshot is not { CallId: { } callId, IsIncoming: true })
            {
                return;
            }

            await callingHub.RejectCallAsync(callId);
            await TeardownAsync(callId);
        }, cancellationToken);

    public Task EndAsync(CancellationToken cancellationToken = default) =>
        RunWhileRunningAsync(async () =>
        {
            if (_snapshot.CallId is not { } callId)
            {
                return;
            }

            await callingHub.EndCallAsync(callId);
            await TeardownAsync(callId);
        }, cancellationToken);

    public Task ReconcileAsync(CancellationToken cancellationToken = default) =>
        RunWhileRunningAsync(async () =>
        {
            ThrowIfDisposed();
            await ReconcileCoreAsync(cancellationToken);
        }, cancellationToken);

    public Task HandleAsync(Call notification) => QueueCallbackAsync(async () =>
    {
        if (notification is CallStateChangedNotification stateChanged)
        {
            if (_snapshot.HasCall && _snapshot.CallId != stateChanged.CallId)
            {
                return;
            }

            SetCall(stateChanged, true);
            await StopRingtonesAsync();
            return;
        }

        if (notification is IncomingCallNotification incoming)
        {
            if (_snapshot.HasCall && _snapshot.CallId != incoming.CallId)
            {
                return;
            }

            SetCall(incoming, false);
            await StopRingtonesAsync();
            if (incoming.Role == CallRole.Receiver)
            {
                _ringtone = await TryStartRingtoneAsync();
            }
            else
            {
                _ringback = await TryStartRingtoneAsync();
            }
            return;
        }

        if (_snapshot.CallId != notification.CallId)
        {
            return;
        }

        var isOnAnotherDevice = _snapshot.IsOnAnotherDevice;
        SetCall(notification, isOnAnotherDevice);
        if (notification is CallRejectedNotification or CallEndedNotification)
        {
            await TeardownAsync(notification.CallId);
            return;
        }

        if (isOnAnotherDevice)
        {
            return;
        }

        switch (notification)
        {
            case CallAcceptedNotification:
                await HandleAcceptedAsync(notification.CallId);
                break;
            case WebRtcOfferNotification offer:
                await HandleOfferAsync(offer);
                break;
            case WebRtcAnswerNotification answer:
                await HandleAnswerAsync(answer);
                break;
            case IceCandidateNotification candidate:
                await HandleIceCandidateAsync(candidate);
                break;
        }
    });

    public Task ShutdownAsync()
    {
        lock (_lifecycleLock)
        {
            return _shutdownTask ??= ShutdownCoreAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ShutdownAsync();
        _operations.Dispose();
    }

    private async Task HandleAcceptedAsync(Guid callId)
    {
        await StopRingtonesAsync();
        await EnsureMediaAsync(CancellationToken.None);
        if (_role != CallRole.Caller || _peer is null)
        {
            return;
        }

        var offer = await _peer.CreateOfferAsync();
        if (_snapshot.CallId == callId)
        {
            await callingHub.SendOfferAsync(callId, offer);
            SetState(CallState.Offered);
        }
    }

    private async Task HandleOfferAsync(WebRtcOfferNotification notification)
    {
        _role = CallRole.Receiver;
        await StopRingtonesAsync();
        await EnsureMediaAsync(CancellationToken.None);
        if (_peer is null)
        {
            return;
        }

        await _peer.SetRemoteOfferAsync(notification.Sdp);
        var answer = await _peer.CreateAnswerAsync();
        if (_snapshot.CallId == notification.CallId)
        {
            await callingHub.SendAnswerAsync(notification.CallId, answer);
            SetState(CallState.Active);
        }
    }

    private async Task HandleAnswerAsync(WebRtcAnswerNotification notification)
    {
        if (_role != CallRole.Caller)
        {
            return;
        }

        await EnsureMediaAsync(CancellationToken.None);
        if (_peer is not null)
        {
            await _peer.SetRemoteAnswerAsync(notification.Sdp);
        }
    }

    private async Task HandleIceCandidateAsync(IceCandidateNotification notification)
    {
        await EnsureMediaAsync(CancellationToken.None);
        if (_peer is null)
        {
            return;
        }

        ushort? lineIndex = notification.SdpMLineIndex is >= 0 and <= ushort.MaxValue
            ? (ushort)notification.SdpMLineIndex.Value
            : null;
        await _peer.AddIceCandidateAsync(new RtcIceCandidate(
            notification.Candidate,
            notification.SdpMid,
            lineIndex));
    }

    private async Task EnsureMediaAsync(CancellationToken cancellationToken)
    {
        if (_peer is not null && _audioSession is not null)
        {
            return;
        }

        var peer = _peer;
        var audioSession = _audioSession;
        var ownsPeer = false;
        var ownsAudioSession = false;
        try
        {
            if (peer is null)
            {
                peer = await peerFactory.CreatePeer(cancellationToken);
                ownsPeer = true;
                peer.IceCandidateCreated += OnIceCandidateCreated;
                peer.StateChanged += OnPeerStateChanged;
                peer.Audio.AudioReceived += OnAudioReceived;
            }

            if (audioSession is null)
            {
                audioSession = await audioHost.RealtimeAudio.CreateSessionAsync(
                    new RealtimeAudioOptions(),
                    cancellationToken);
                ownsAudioSession = true;
                audioSession.AudioCaptured += OnAudioCaptured;
                audioSession.Faulted += OnAudioSessionFaulted;
                await audioSession.StartAsync(cancellationToken);
            }

            _peer = peer;
            _audioSession = audioSession;
        }
        catch
        {
            if (ownsAudioSession && audioSession is not null)
            {
                audioSession.AudioCaptured -= OnAudioCaptured;
                audioSession.Faulted -= OnAudioSessionFaulted;
                await TryCleanupAsync(async () => await audioSession.StopAsync(), "stop partially started realtime audio");
                await TryCleanupAsync(async () => await audioSession.DisposeAsync(), "dispose partially started realtime audio");
            }

            if (ownsPeer && peer is not null)
            {
                peer.IceCandidateCreated -= OnIceCandidateCreated;
                peer.StateChanged -= OnPeerStateChanged;
                peer.Audio.AudioReceived -= OnAudioReceived;
                await TryCleanupAsync(peer.CloseAsync, "close a partially started RTC peer");
                await TryCleanupAsync(async () => await peer.DisposeAsync(), "dispose a partially started RTC peer");
            }

            if (_snapshot.CallId is { } callId)
            {
                await TryCleanupAsync(
                    () => callingHub.EndCallAsync(callId),
                    "end a call after media startup failed");
            }

            await TeardownAsync();
            throw;
        }
    }

    private void OnAudioCaptured(ReadOnlyMemory<float> samples)
    {
        try
        {
            _peer?.Audio.Send(samples.Span);
        }
        catch (Exception exception)
        {
            LogAudioCallbackFailed(exception);
        }
    }

    private void OnAudioReceived(ReadOnlyMemory<float> samples)
    {
        try
        {
            _audioSession?.TryWritePlayback(samples.Span);
        }
        catch (Exception exception)
        {
            LogAudioCallbackFailed(exception);
        }
    }

    private void OnIceCandidateCreated(object? sender, IceCandidateCreatedEventArgs eventArgs)
    {
        var peer = sender as IWebRtcPeer;
        QueueCallback(async () =>
        {
            if (peer != _peer || _snapshot.CallId is not { } callId)
            {
                return;
            }

            var candidate = eventArgs.Candidate;
            await callingHub.SendIceCandidateAsync(
                callId,
                candidate.Candidate,
                candidate.SdpMid,
                candidate.SdpMLineIndex);
        }, "relay a local ICE candidate");
    }

    private void OnPeerStateChanged(object? sender, PeerConnectionStateChangedEventArgs eventArgs)
    {
        var peer = sender as IWebRtcPeer;
        QueueCallback(async () =>
        {
            if (peer != _peer || _snapshot.CallId is not { } callId)
            {
                return;
            }

            if (eventArgs.State == PeerConnectionState.Connected)
            {
                SetState(CallState.Active);
                return;
            }

            if (eventArgs.State is not (PeerConnectionState.Failed or PeerConnectionState.Closed))
            {
                return;
            }

            try
            {
                await callingHub.EndCallAsync(callId);
            }
            catch (Exception exception)
            {
                LogOperationFailed(exception, "end a failed or closed call");
                return;
            }

            await TeardownAsync(callId);
        }, "handle a peer state change");
    }

    private void OnAudioSessionFaulted(object? sender, AudioSessionFaultedEventArgs eventArgs)
    {
        var session = sender as IRealtimeAudioSession;
        QueueCallback(async () =>
        {
            if (session != _audioSession || _snapshot.CallId is not { } callId)
            {
                return;
            }

            LogOperationFailed(eventArgs.Exception, "run realtime audio");
            try
            {
                await callingHub.EndCallAsync(callId);
            }
            catch (Exception exception)
            {
                LogOperationFailed(exception, "end a call after an audio fault");
                return;
            }

            await TeardownAsync(callId);
        }, "handle a realtime audio fault");
    }

    private async Task TeardownAsync(Guid? expectedCallId = null)
    {
        if (expectedCallId.HasValue && _snapshot.CallId != expectedCallId)
        {
            return;
        }

        var peer = _peer;
        var audioSession = _audioSession;
        _peer = null;
        _audioSession = null;
        _role = null;
        Publish(CallSnapshot.Empty);

        await StopRingtonesAsync();

        if (audioSession is not null)
        {
            audioSession.AudioCaptured -= OnAudioCaptured;
            audioSession.Faulted -= OnAudioSessionFaulted;
            await TryCleanupAsync(async () => await audioSession.StopAsync(), "stop realtime audio");
            await TryCleanupAsync(async () => await audioSession.DisposeAsync(), "dispose realtime audio");
        }

        if (peer is not null)
        {
            peer.IceCandidateCreated -= OnIceCandidateCreated;
            peer.StateChanged -= OnPeerStateChanged;
            peer.Audio.AudioReceived -= OnAudioReceived;
            await TryCleanupAsync(peer.CloseAsync, "close the RTC peer");
            await TryCleanupAsync(async () => await peer.DisposeAsync(), "dispose the RTC peer");
        }
    }

    private void SetCall(CallInfo call)
    {
        _role = call.Role;
        Publish(new CallSnapshot(
            call.CallId,
            call.RemoteUserId,
            call.RemoteUsername,
            call.State,
            call.Role == CallRole.Receiver,
            false));
    }

    private void SetCall(Call call, bool isOnAnotherDevice)
    {
        _role = call.Role;
        Publish(new CallSnapshot(
            call.CallId,
            call.RemoteUserId,
            call.RemoteUsername,
            call.State,
            call.Role == CallRole.Receiver,
            isOnAnotherDevice));
    }

    private void SetState(CallState state) => Publish(_snapshot with { State = state });

    private void Publish(CallSnapshot snapshot)
    {
        Volatile.Write(ref _snapshot, snapshot);
        var subscribers = SnapshotChanged?.GetInvocationList();
        if (subscribers is null)
        {
            return;
        }

        foreach (var subscriber in subscribers)
        {
            try
            {
                ((Action<CallSnapshot>)subscriber)(snapshot);
            }
            catch (Exception exception)
            {
                LogOperationFailed(exception, "publish a call snapshot");
            }
        }
    }

    private async Task<IAudioPlayback?> TryStartRingtoneAsync()
    {
        try
        {
            return await soundPlayer.PlayRingtoneAsync();
        }
        catch (Exception exception)
        {
            LogOperationFailed(exception, "start call ringing audio");
            return null;
        }
    }

    private async Task StopRingtonesAsync()
    {
        var ringtone = _ringtone;
        var ringback = _ringback;
        _ringtone = null;
        _ringback = null;
        await StopPlaybackAsync(ringtone);
        await StopPlaybackAsync(ringback);
    }

    private async Task StopPlaybackAsync(IAudioPlayback? playback)
    {
        if (playback is null)
        {
            return;
        }

        await TryCleanupAsync(async () => await playback.StopAsync(), "stop call ringing audio");
        try
        {
            playback.Dispose();
        }
        catch (Exception exception)
        {
            LogOperationFailed(exception, "dispose call ringing audio");
        }
    }

    private async Task RunSerializedAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await _operations.WaitAsync(cancellationToken);
        try
        {
            await operation();
        }
        finally
        {
            _operations.Release();
        }
    }

    private Task RunWhileRunningAsync(Func<Task> operation, CancellationToken cancellationToken)
    {
        lock (_lifecycleLock)
        {
            if (_stopping != 0)
            {
                return Task.CompletedTask;
            }

            return RunSerializedAsync(operation, cancellationToken);
        }
    }

    private async Task TryCleanupAsync(Func<Task> cleanup, string operation)
    {
        try
        {
            await cleanup();
        }
        catch (Exception exception)
        {
            LogOperationFailed(exception, operation);
        }
    }

    private async Task ReconcileCoreAsync(CancellationToken cancellationToken)
    {
        var call = await callingHub.GetCurrentCallAsync();
        if (call is null || call.State == CallState.Ended)
        {
            await TeardownAsync();
            return;
        }

        if (call.State is CallState.Offered or CallState.Active)
        {
            await callingHub.EndCallAsync(call.CallId);
            await TeardownAsync();
            return;
        }

        if (_snapshot.CallId != call.CallId)
        {
            await TeardownAsync();
        }

        SetCall(call);
        await StopRingtonesAsync();
        if (call.State == CallState.Ringing)
        {
            if (call.Role == CallRole.Receiver)
            {
                _ringtone = await TryStartRingtoneAsync();
            }
            else
            {
                _ringback = await TryStartRingtoneAsync();
            }

            return;
        }

        await EnsureMediaAsync(cancellationToken);
        if (call.Role == CallRole.Caller)
        {
            await HandleAcceptedAsync(call.CallId);
        }
    }

    private async Task ShutdownCoreAsync()
    {
        Interlocked.Exchange(ref _stopping, 1);
        await RunSerializedAsync(async () =>
        {
            if (_snapshot.CallId is { } callId)
            {
                try
                {
                    await callingHub.EndCallAsync(callId);
                }
                catch (Exception exception)
                {
                    LogOperationFailed(exception, "end the call during shutdown");
                }
            }

            await TeardownAsync();
        });

        Task[] callbacks;
        lock (_lifecycleLock)
        {
            callbacks = [.. _callbackTasks];
        }

        await Task.WhenAll(callbacks);
        _disposed = true;
    }

    private Task QueueCallbackAsync(Func<Task> operation)
    {
        Task task;
        lock (_lifecycleLock)
        {
            if (_stopping != 0)
            {
                return Task.CompletedTask;
            }

            task = RunSerializedAsync(operation);
            _callbackTasks.Add(task);
        }

        _ = RemoveCallbackWhenCompleteAsync(task);
        return task;
    }

    private void QueueCallback(Func<Task> operation, string description) =>
        _ = ObserveAsync(QueueCallbackAsync(operation), description);

    private async Task RemoveCallbackWhenCompleteAsync(Task task)
    {
        try
        {
            await task;
        }
        finally
        {
            lock (_lifecycleLock)
            {
                _callbackTasks.Remove(task);
            }
        }
    }

    private async Task ObserveAsync(Task task, string operation)
    {
        try
        {
            await task;
        }
        catch (Exception exception)
        {
            LogOperationFailed(exception, operation);
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    [LoggerMessage(LogLevel.Warning, "Failed to {Operation}.")]
    private partial void LogOperationFailed(Exception exception, string operation);

    [LoggerMessage(LogLevel.Debug, "An audio callback failed.")]
    private partial void LogAudioCallbackFailed(Exception exception);

}
