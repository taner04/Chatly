using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels;

public sealed partial class SplashScreenViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string StartupMessage { get; set; } = "Authenticating...";

    [RelayCommand]
    private void Cancel()
    {
        StartupMessage = "Cancelling...";
        _cts.Cancel();
    }

    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public CancellationToken CancellationToken => _cts.Token;
}