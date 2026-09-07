using System.Linq;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.ViewModels.Toasts;

namespace Chatly.Desktop.Views.Toasts;

[SingletonService]
public partial class ToastHostOverlay : UserControl, IToastHost
{
    public ToastHostOverlay(ToastHostOverlayViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public ToastHostOverlayViewModel ViewModel { get; }

    public void AddToast(IToastViewModel toastViewModel)
    {
        ViewModel.Toasts.Insert(0, toastViewModel);
    }

    public void RemoveToast(Guid id)
    {
        if (ViewModel.Toasts.FirstOrDefault(t => t.Id == id) is { } toast)
        {
            ViewModel.Toasts.Remove(toast);
        }
    }
}