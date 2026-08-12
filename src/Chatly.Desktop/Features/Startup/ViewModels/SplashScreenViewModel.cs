using System.Threading;
using Chatly.Desktop.Shared.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.Features.Startup.ViewModels;

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