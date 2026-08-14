using Avalonia.Controls;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.ViewModels.Pages;

namespace Chatly.Desktop.Views.Pages;

public partial class UserInfoPage : UserControl, INavigablePage
{
    public UserInfoPage(UserInfoPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public UserInfoPageViewModel ViewModel { get; }
}
