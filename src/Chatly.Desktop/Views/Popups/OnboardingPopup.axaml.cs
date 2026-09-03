using OnboardingPopupViewModel = Chatly.Desktop.ViewModels.Popups.OnboardingPopupViewModel;

namespace Chatly.Desktop.Views.Popups;

[TransientService(typeof(IPopupOverlay<OnboardingPopupViewModel>))]
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