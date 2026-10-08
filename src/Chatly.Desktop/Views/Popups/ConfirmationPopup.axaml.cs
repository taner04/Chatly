using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.Views.Popups;

internal partial class ConfirmationPopup : UserControl, IViewFor<ConfirmationPopupViewModel>
{
    public ConfirmationPopup(ConfirmationPopupViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public ConfirmationPopupViewModel ViewModel { get; }
}