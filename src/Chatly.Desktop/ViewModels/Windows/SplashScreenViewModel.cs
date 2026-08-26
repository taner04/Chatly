using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Windows;

public sealed partial class SplashScreenViewModel : ViewModelBase
{
    private readonly CancellationTokenSource _cts = new();

    [ObservableProperty] public partial string StartupMessage { get; set; } = "Authenticating...";

    public CancellationToken CancellationToken => _cts.Token;

    [RelayCommand]
    private void Cancel()
    {
        StartupMessage = "Cancelling...";
        _cts.Cancel();
    }
}