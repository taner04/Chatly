using Avalonia.Controls;
using Chatly.Desktop.Features.Home.ViewModels;
using Chatly.Desktop.Shared.Abstractions;

namespace Chatly.Desktop.Features.Home.Views;

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
