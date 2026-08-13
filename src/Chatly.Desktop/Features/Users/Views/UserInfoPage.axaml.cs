using Avalonia.Controls;
using Chatly.Desktop.Features.Users.ViewModels;
using Chatly.Desktop.Shared.Abstractions;

namespace Chatly.Desktop;

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
