using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Windows;

[SingletonService]
public sealed partial class SplashScreenViewModel : ViewModelBase
{
    private readonly CancellationTokenSource _cts = new();

    [ObservableProperty] public partial string StartupMessage { get; set; } = "Authenticating...";

    internal CancellationToken CancellationToken => _cts.Token;

    [RelayCommand]
    private void Cancel()
    {
        StartupMessage = "Cancelling...";
        _cts.Cancel();
    }
}
