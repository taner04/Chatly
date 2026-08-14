using Avalonia.Controls;
using Chatly.Desktop.Abstractions.Overlays;
using Chatly.Desktop.ViewModels.Overlays;

namespace Chatly.Desktop.Views.Overlays;

public partial class OnboardingOverlay : UserControl, IPopupOverlay<OnboardingOverlayViewModel>
{
    public OnboardingOverlay(OnboardingOverlayViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public OnboardingOverlayViewModel ViewModel { get; }
}
