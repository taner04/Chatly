using Chatly.Desktop.ViewModels.Toasts;

namespace Chatly.Desktop.Views.Toasts;

[SingletonService]
internal partial class ToastHostOverlay : UserControl
{
    public ToastHostOverlay(ToastHostOverlayViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public ToastHostOverlayViewModel ViewModel { get; }
}