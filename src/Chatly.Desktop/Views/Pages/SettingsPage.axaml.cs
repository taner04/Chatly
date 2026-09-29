using Chatly.Desktop.ViewModels.Pages;

namespace Chatly.Desktop.Views.Pages;

internal partial class SettingsPage : UserControl, IViewFor<SettingsPageViewModel>
{
    public SettingsPage(SettingsPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public SettingsPageViewModel ViewModel { get; }
}