using Chatly.Desktop.ViewModels.Pages;

namespace Chatly.Desktop.Views.Pages;

[SingletonService(typeof(INavigableView<SettingsPageViewModel>))]
public partial class SettingsPage : UserControl, INavigableView<SettingsPageViewModel>
{
    public SettingsPage(SettingsPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public SettingsPageViewModel ViewModel { get; }
}