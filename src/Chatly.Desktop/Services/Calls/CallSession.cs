using System.Runtime.CompilerServices;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using System.ComponentModel;
using Chatly.Desktop.Abstraction.Calls;
using Chatly.Desktop.Models.Calls;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Calls;

[SingletonService]
public sealed partial class CallSession
{
    private readonly CallSettings _callSettings;
    private readonly ICallingHubServer _callingHub;
    private readonly ILogger<CallSession> _logger;
    private readonly ICallMediaHost _mediaHost;
    private readonly CallOperationQueue _operations = new();
    private readonly CallToneController _tones;
    private Guid? _mediaCallId;
    private CallSnapshot _snapshot = CallSnapshot.Empty;

    public CallSession(
        ICallingHubServer callingHub,
        ICallMediaHost mediaHost,
        CallToneController tones,
        AppSettings appSettings,
        ILogger<CallSession> logger)
    {
        _callSettings = appSettings.CallSettings;
        _callingHub = callingHub;
        _mediaHost = mediaHost;
        _tones = tones;
        _logger = logger;
        _mediaHost.RemoteParticipantJoined += OnRemoteParticipantJoined;
        _mediaHost.RemoteParticipantLeft += OnRemoteParticipantLeft;
        _mediaHost.Disconnected += OnMediaDisconnected;
        _mediaHost.ReconnectingChanged += OnMediaReconnectingChanged;
        _callSettings.PropertyChanged += OnCallSettingsChanged;
    }

    internal CallRole? Role { get; private set; }

    internal CallSnapshot Snapshot => Volatile.Read(ref _snapshot);

    internal event Action<CallSnapshot>? SnapshotChanged;

    internal Task RunAsync(Func<Task> operation, CancellationToken cancellationToken = default) =>
        _operations.RunAsync(operation, cancellationToken);

    internal Task EnqueueAsync(Func<Task> operation) => _operations.EnqueueAsync(operation);

    internal Task StopAsync(Func<Task> finalOperation) => _operations.StopAsync(finalOperation);

    internal bool HasDifferentCall(Guid callId) =>
        _snapshot.HasCall && _snapshot.CallId != callId;

    internal bool TryApplyCurrentNotification(
        CallMessage notification,
        out bool isOnAnotherDevice)
    {
        isOnAnotherDevice = false;
        if (_snapshot.CallId != notification.CallId)
        {
            return false;
        }

        isOnAnotherDevice = _snapshot.IsOnAnotherDevice;
        SetCall(notification, isOnAnotherDevice);
        return true;
    }

    internal void SetCall(CallInfo call) =>
        SetCall(call.CallId, call.RemoteUserId, call.RemoteUsername, call.State, call.Role, false, call.AcceptedAt);

    internal void SetCall(CallMessage call, bool isOnAnotherDevice) =>
        SetCall(
            call.CallId,
            call.RemoteUserId,
            call.RemoteUsername,
            call.State,
            call.Role,
            isOnAnotherDevice,
            call switch
            {
                CallAcceptedNotification accepted => accepted.AcceptedAt,
                CallStateChangedNotification changed => changed.AcceptedAt,
                _ => null
            });

    internal void SetState(CallState state) => Publish(_snapshot with { State = state });

    internal async Task SetMutedAsync(bool muted)
    {
        if (_mediaCallId is not { } callId || _snapshot.CallId != callId)
        {
            return;
        }

        await _mediaHost.SetMicrophoneEnabledAsync(!muted);
        Publish(_snapshot with { IsMuted = muted });
    }

    private void SetCall(
        Guid callId,
        Guid remoteUserId,
        string? remoteUsername,
        CallState state,
        CallRole role,
        bool isOnAnotherDevice,
        DateTimeOffset? acceptedAt)
    {
        Role = role;
        var current = _snapshot.CallId == callId ? _snapshot : null;
        Publish(new CallSnapshot(
            callId,
            remoteUserId,
            remoteUsername,
            state,
            role == CallRole.Receiver,
            isOnAnotherDevice,
            current?.IsMediaConnected ?? false,
            acceptedAt ?? current?.AcceptedAt,
            current?.IsMuted ?? false,
            current?.IsReconnecting ?? false));
    }

    internal Task StartIncomingToneAsync() => _tones.StartIncomingAsync();

    internal Task StartOutgoingToneAsync() => _tones.StartOutgoingAsync();

    internal Task StopTonesAsync() => _tones.StopAsync();

    internal async Task EnsureMediaAsync(CancellationToken cancellationToken)
    {
        if (_snapshot.CallId is not { } callId)
        {
            throw new InvalidOperationException("There is no call to join.");
        }

        if (_mediaCallId == callId)
        {
            return;
        }

        try
        {
            var access = await _callingHub.JoinMediaAsync(callId);
            _mediaCallId = callId;
            await _mediaHost.JoinAsync(access.ServerUrl, access.Token, CreateAudioOptions(), cancellationToken);
        }
        catch
        {
            await CallCoordinatorUtilities.TryInvokeAsync(
                () => _callingHub.EndCallAsync(callId),
                LogOperationFailed);
            await TeardownAsync(callId);
            throw;
        }
    }

    internal async Task TeardownAsync(Guid? expectedCallId = null)
    {
        if (expectedCallId.HasValue && _snapshot.CallId != expectedCallId)
        {
            return;
        }

        var hadMedia = _mediaCallId is not null;
        _mediaCallId = null;
        Role = null;
        Publish(CallSnapshot.Empty);

        await _tones.StopAsync();

        if (hadMedia)
        {
            await CallCoordinatorUtilities.TryInvokeAsync(_mediaHost.LeaveAsync, LogOperationFailed);
        }
    }

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
                LogOperationFailed(exception);
            }
        }
    }

    private void OnRemoteParticipantJoined(object? sender, EventArgs e) =>
        QueueCallback(() =>
        {
            if (_mediaCallId is { } callId && _snapshot.CallId == callId)
            {
                Publish(_snapshot with { IsMediaConnected = true });
            }

            return Task.CompletedTask;
        });

    private void OnRemoteParticipantLeft(object? sender, EventArgs e) => QueueCallback(EndMediaCallAsync);

    private void OnMediaDisconnected(object? sender, string reason) => QueueCallback(EndMediaCallAsync);

    private void OnCallSettingsChanged(object? sender, PropertyChangedEventArgs e) =>
        QueueCallback(async () =>
        {
            if (_mediaCallId is { } callId && _snapshot.CallId == callId)
            {
                await _mediaHost.ApplyAudioOptionsAsync(CreateAudioOptions());
            }
        });

    private CallAudioOptions CreateAudioOptions() => new(
        _callSettings.InputDeviceId,
        _callSettings.OutputDeviceId,
        _callSettings.EchoCancellation,
        _callSettings.NoiseSuppression,
        _callSettings.AutoGainControl);

    private void OnMediaReconnectingChanged(object? sender, bool isReconnecting) =>
        QueueCallback(() =>
        {
            if (_mediaCallId is { } callId && _snapshot.CallId == callId)
            {
                Publish(_snapshot with { IsReconnecting = isReconnecting });
            }

            return Task.CompletedTask;
        });

    private async Task EndMediaCallAsync()
    {
        if (_mediaCallId is not { } callId || _snapshot.CallId != callId)
        {
            return;
        }

        await CallCoordinatorUtilities.TryInvokeAsync(
            () => _callingHub.EndCallAsync(callId),
            LogOperationFailed);
        await TeardownAsync(callId);
    }

    private void QueueCallback(
        Func<Task> operation,
        [CallerMemberName] string caller = "") =>
        _ = CallCoordinatorUtilities.TryInvokeAsync(
            () => _operations.EnqueueAsync(operation),
            LogOperationFailed,
            caller);

    [LoggerMessage(LogLevel.Warning, "Call session operation {Operation} failed.")]
    private partial void LogOperationFailed(
        Exception exception,
        [CallerMemberName] string operation = "");
}
