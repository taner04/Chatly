using Avalonia.Controls;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.ViewModels.Pages;

namespace Chatly.Desktop.Views.Pages;

public partial class HomePage : UserControl, INavigablePage
{
    public HomePage(HomePageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;

        InitializeComponent();
    }

    public HomePageViewModel ViewModel { get; }
}
