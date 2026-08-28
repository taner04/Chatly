using Avalonia.Controls;
using Avalonia.Input;
using Chatly.Desktop.Abstractions.Popups;
using OnboardingPopupViewModel = Chatly.Desktop.ViewModels.Popups.OnboardingPopupViewModel;

namespace Chatly.Desktop.Views.Popups;

public partial class OnboardingPopup : UserControl, IPopupOverlay<OnboardingPopupViewModel>
{
    public OnboardingPopup(OnboardingPopupViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public OnboardingPopupViewModel ViewModel { get; }
}
