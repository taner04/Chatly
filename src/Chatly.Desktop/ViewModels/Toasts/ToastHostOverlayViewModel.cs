using System.Collections.ObjectModel;
using Chatly.Desktop.Abstraction.Toasts;

namespace Chatly.Desktop.ViewModels.Toasts;

[SingletonService]
public sealed partial class ToastHostOverlayViewModel : ViewModelBase, IToastHostViewModel
{
    [ObservableProperty] public partial ObservableCollection<IToastViewModel> Toasts { get; set; } = [];
}