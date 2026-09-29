using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.Views.Popups;

internal partial class OnboardingPopup : UserControl, IViewFor<OnboardingPopupViewModel>
{
    public OnboardingPopup(OnboardingPopupViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public OnboardingPopupViewModel ViewModel { get; }
}