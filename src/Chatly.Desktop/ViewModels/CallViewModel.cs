using Avalonia.Threading;
using Chatly.Desktop.Services.Calls;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.ViewModels;

[SingletonService]
public sealed partial class CallViewModel : ViewModelBase, IDisposable
{
    private readonly CallCoordinator _coordinator;
    private readonly DispatcherTimer _durationTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly ILogger<CallViewModel> _logger;
    private readonly IToastService _toastService;
    private readonly UserRegistry _userRegistry;
    private readonly UserSessionContext _userSessionContext;

    [ObservableProperty] private DateTimeOffset? _acceptedAt;

    [ObservableProperty] private Guid? _callId;

    [ObservableProperty] private string _durationText = string.Empty;

    [ObservableProperty] private bool _hasCall;

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private bool _isIncoming;

    [ObservableProperty] private bool _isMediaConnected;

    [ObservableProperty] private bool _isMuted;

    [ObservableProperty] private bool _isOnAnotherDevice;

    [ObservableProperty] private bool _isReconnecting;

    [ObservableProperty] private User? _remoteUser;

    [ObservableProperty] private Guid? _remoteUserId;

    [ObservableProperty] private string? _remoteUsername;

    [ObservableProperty] private CallState? _state;

    public CallViewModel(
        CallCoordinator coordinator,
        IToastService toastService,
        ILogger<CallViewModel> logger,
        UserSessionContext userSessionContext,
        UserRegistry userRegistry)
    {
        _coordinator = coordinator;
        _toastService = toastService;
        _logger = logger;
        _userSessionContext = userSessionContext;
        _userRegistry = userRegistry;
        _durationTimer.Tick += (_, _) => UpdateDuration();
        _coordinator.SnapshotChanged += OnSnapshotChanged;
        ApplySnapshot(_coordinator.Snapshot);
    }

    public bool CanAccept => HasCall && IsIncoming && State == CallState.Ringing && !IsBusy && !IsOnAnotherDevice;

    public bool CanDecline => CanAccept;

    public bool CanCancel => HasCall && !IsIncoming && State == CallState.Ringing && !IsBusy && !IsOnAnotherDevice;

    public bool CanLeave => HasCall && State == CallState.Active && !IsBusy && !IsOnAnotherDevice;

    public bool CanToggleMute => CanLeave && IsMediaConnected;

    public bool ShowDuration => State == CallState.Active && AcceptedAt.HasValue && !IsOnAnotherDevice;

    public string StatusText => IsOnAnotherDevice
        ? "Call in progress on another device"
        : State switch
        {
            CallState.Ringing when IsIncoming => "Incoming call",
            CallState.Ringing => "Calling...",
            CallState.Active when IsReconnecting => "Reconnecting...",
            CallState.Active when IsMediaConnected => "In call",
            CallState.Active => "Connecting...",
            CallState.Ended => "Call ended",
            _ => string.Empty
        };

    public void Dispose()
    {
        _durationTimer.Stop();
        _coordinator.SnapshotChanged -= OnSnapshotChanged;
    }

    [RelayCommand(CanExecute = nameof(CanStartCall))]
    private async Task CallAsync(User? user, CancellationToken cancellationToken)
    {
        var caller = _userSessionContext.CurrentUser;
        if (user is null || caller is null || user.Id == caller.Id)
        {
            return;
        }

        await RunCallOperationAsync(
            () => _coordinator.StartCallAsync(user.Id, cancellationToken),
            cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanAccept))]
    private Task AcceptAsync(CancellationToken cancellationToken) =>
        RunCallOperationAsync(
            () => _coordinator.AcceptAsync(cancellationToken),
            cancellationToken);

    [RelayCommand(CanExecute = nameof(CanDecline))]
    private Task DeclineAsync(CancellationToken cancellationToken) =>
        RunCallOperationAsync(
            () => _coordinator.RejectAsync(cancellationToken),
            cancellationToken);

    [RelayCommand(CanExecute = nameof(CanToggleMute))]
    private Task ToggleMuteAsync(CancellationToken cancellationToken) =>
        RunCallOperationAsync(
            () => _coordinator.SetMutedAsync(!IsMuted, cancellationToken),
            cancellationToken);

    [RelayCommand(CanExecute = nameof(CanEndCall))]
    private Task LeaveCallAsync(CancellationToken cancellationToken) =>
        RunCallOperationAsync(
            () => _coordinator.EndAsync(cancellationToken),
            cancellationToken);

    private void OnSnapshotChanged(CallSnapshot snapshot)
    {
        UiThreadDispatcher.SafeInvoke(() => ApplySnapshot(snapshot));
    }

    private void ApplySnapshot(CallSnapshot snapshot)
    {
        CallId = snapshot.CallId;
        RemoteUserId = snapshot.RemoteUserId;
        RemoteUsername = snapshot.RemoteUsername;
        State = snapshot.State;
        IsIncoming = snapshot.IsIncoming;
        HasCall = snapshot.HasCall;
        IsOnAnotherDevice = snapshot.IsOnAnotherDevice;
        IsMediaConnected = snapshot.IsMediaConnected;
        IsMuted = snapshot.IsMuted;
        IsReconnecting = snapshot.IsReconnecting;
        AcceptedAt = snapshot.AcceptedAt;
        RemoteUser = snapshot.RemoteUserId is { } remoteUserId ? _userRegistry.Find(remoteUserId) : null;
        UpdateDuration();
        RefreshControls();
    }

    private bool CanEndCall() => CanCancel || CanLeave;

    private void UpdateDuration()
    {
        if (!ShowDuration)
        {
            _durationTimer.Stop();
            DurationText = string.Empty;
            return;
        }

        var elapsed = DateTimeOffset.UtcNow - AcceptedAt!.Value;
        if (elapsed < TimeSpan.Zero)
        {
            elapsed = TimeSpan.Zero;
        }

        DurationText = elapsed.TotalHours >= 1
            ? elapsed.ToString(@"h\:mm\:ss")
            : elapsed.ToString(@"mm\:ss");
        if (!_durationTimer.IsEnabled)
        {
            _durationTimer.Start();
        }
    }

    private bool CanStartCall(User? user) => user is not null && !HasCall && !IsBusy;

    private async Task RunCallOperationAsync(
        Func<Task> operation,
        CancellationToken cancellationToken)
    {
        IsBusy = true;
        RefreshControls();
        try
        {
            await operation();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            LogCallOperationFailed(exception);
            _toastService.ShowError("The call operation failed. Please try again.");
        }
        finally
        {
            IsBusy = false;
            RefreshControls();
        }
    }

    private void RefreshControls()
    {
        OnPropertyChanged(nameof(CanAccept));
        OnPropertyChanged(nameof(CanDecline));
        OnPropertyChanged(nameof(CanCancel));
        OnPropertyChanged(nameof(CanLeave));
        OnPropertyChanged(nameof(CanToggleMute));
        OnPropertyChanged(nameof(ShowDuration));
        OnPropertyChanged(nameof(StatusText));
        CallCommand.NotifyCanExecuteChanged();
        AcceptCommand.NotifyCanExecuteChanged();
        DeclineCommand.NotifyCanExecuteChanged();
        LeaveCallCommand.NotifyCanExecuteChanged();
        ToggleMuteCommand.NotifyCanExecuteChanged();
    }

    [LoggerMessage(LogLevel.Error, "Call operation failed.")]
    private partial void LogCallOperationFailed(Exception exception);
}