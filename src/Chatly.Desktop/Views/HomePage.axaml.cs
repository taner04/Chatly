using Avalonia.Controls;
using Chatly.Desktop.Abstractions;
using Chatly.Desktop.ViewModels;

namespace Chatly.Desktop.Views;

public partial class HomePage : UserControl, INavigavablePage
{
    public HomePage(HomePageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        
        InitializeComponent();
    }
    
    public HomePageViewModel ViewModel { get; }
}
