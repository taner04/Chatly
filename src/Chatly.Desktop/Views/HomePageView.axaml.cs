using Avalonia.Controls;
using Chatly.Desktop.Abstractions;
using Chatly.Desktop.ViewModels;

namespace Chatly.Desktop.Views;

public partial class HomePageView : UserControl, INavigavablePage
{
    public HomePageView(HomePageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        
        InitializeComponent();
    }
    
    public HomePageViewModel ViewModel { get; }
}
