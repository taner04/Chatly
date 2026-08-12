using Avalonia.Controls;
using Chatly.Desktop.Features.Onboarding.ViewModels;
using Chatly.Desktop.Shared.Abstractions;

namespace Chatly.Desktop.Features.Onboarding.Views;

public partial class OnboardingPage : UserControl, INavigavablePage
{
    public OnboardingPage(OnboardingPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
    }
    
    public OnboardingPageViewModel ViewModel { get;}
}