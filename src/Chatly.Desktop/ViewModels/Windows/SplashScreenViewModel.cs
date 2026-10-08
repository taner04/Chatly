namespace Chatly.Desktop.ViewModels.Windows;

[SingletonService]
public sealed partial class SplashScreenViewModel : ViewModelBase, IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private TaskCompletionSource? _retry;

    [ObservableProperty] public partial string StartupMessage { get; set; } = "Authenticating...";

    [ObservableProperty] public partial bool HasError { get; private set; }

    internal CancellationToken CancellationToken => _cts.Token;

    public void Dispose()
    {
        _cts.Dispose();
    }

    internal async Task WaitForRetryAsync(string message, CancellationToken cancellationToken)
    {
        var retry = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _retry = retry;
        StartupMessage = message;
        HasError = true;

        try
        {
            await using (cancellationToken.Register(() => retry.TrySetCanceled(cancellationToken)))
            {
                await retry.Task;
            }
        }
        finally
        {
            HasError = false;
            _retry = null;
        }
    }

    [RelayCommand]
    private void Retry() => _retry?.TrySetResult();

    [RelayCommand]
    private void Cancel()
    {
        StartupMessage = "Cancelling...";
        _cts.Cancel();
    }
}