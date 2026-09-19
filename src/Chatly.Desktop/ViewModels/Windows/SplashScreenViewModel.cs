namespace Chatly.Desktop.ViewModels.Windows;

[SingletonService]
public sealed partial class SplashScreenViewModel : ViewModelBase, IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    [ObservableProperty] public partial string StartupMessage { get; set; } = "Authenticating...";

    internal CancellationToken CancellationToken => _cts.Token;

    public void Dispose()
    {
        _cts.Dispose();
    }

    [RelayCommand]
    private void Cancel()
    {
        StartupMessage = "Cancelling...";
        _cts.Cancel();
    }
}