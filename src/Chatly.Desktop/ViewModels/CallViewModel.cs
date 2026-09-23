using Chatly.Desktop.Services.Calls;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.ViewModels;

[SingletonService]
public sealed partial class CallViewModel : ViewModelBase, IDisposable
{
    private readonly CallCoordinator _coordinator;
    private readonly IToastService _toastService;
    private readonly ILogger<CallViewModel> _logger;
    private readonly UserSessionContext _userSessionContext;

    [ObservableProperty]
    private Guid? _callId;

    [ObservableProperty]
    private Guid? _remoteUserId;

    [ObservableProperty]
    private string? _remoteUsername;

    [ObservableProperty]
    private CallState? _state;

    [ObservableProperty]
    private bool _isIncoming;

    [ObservableProperty]
    private bool _hasCall;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOnAnotherDevice;

    public bool CanAccept => HasCall && IsIncoming && State == CallState.Ringing && !IsBusy && !IsOnAnotherDevice;

    public bool CanDecline => CanAccept;

    public bool CanLeave => HasCall && !(IsIncoming && State == CallState.Ringing) && !IsBusy && !IsOnAnotherDevice;

    public string StatusText => IsOnAnotherDevice ? "Call in progress on another device" : State switch
    {
        CallState.Ringing when IsIncoming => "Incoming call",
        CallState.Ringing => "Calling...",
        CallState.Accepted or CallState.Offered => "Connecting...",
        CallState.Active => "In call",
        CallState.Ended => "Call ended",
        _ => string.Empty
    };

    public CallViewModel(
        CallCoordinator coordinator,
        IToastService toastService,
        ILogger<CallViewModel> logger,
        UserSessionContext userSessionContext)
    {
        _coordinator = coordinator;
        _toastService = toastService;
        _logger = logger;
        _userSessionContext = userSessionContext;
        _coordinator.SnapshotChanged += OnSnapshotChanged;
        ApplySnapshot(_coordinator.Snapshot);
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

    [RelayCommand(CanExecute = nameof(CanLeave))]
    private Task LeaveCallAsync(CancellationToken cancellationToken) =>
        RunCallOperationAsync(
            () => _coordinator.EndAsync(cancellationToken),
            cancellationToken);

    public void Dispose()
    {
        _coordinator.SnapshotChanged -= OnSnapshotChanged;
    }

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
        RefreshControls();
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
        OnPropertyChanged(nameof(CanLeave));
        OnPropertyChanged(nameof(StatusText));
        CallCommand.NotifyCanExecuteChanged();
        AcceptCommand.NotifyCanExecuteChanged();
        DeclineCommand.NotifyCanExecuteChanged();
        LeaveCallCommand.NotifyCanExecuteChanged();
    }

    [LoggerMessage(LogLevel.Error, "Call operation failed.")]
    private partial void LogCallOperationFailed(Exception exception);
}
