using System.Collections.ObjectModel;

namespace Chatly.Desktop.ViewModels.Toasts;

[SingletonService]
public sealed class ToastHostOverlayViewModel : ViewModelBase
{
    public ObservableCollection<IToastViewModel> Toasts { get; } = [];
}