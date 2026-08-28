using Chatly.Desktop.Abstractions.Toasts;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Chatly.Desktop.ViewModels.Toasts;

public sealed partial class ToastHostOverlayViewModel : ViewModelBase, IToastHostViewModel
{
    [ObservableProperty] public partial ObservableCollection<IToastViewModel> Toasts { get; set; } = [];
}
