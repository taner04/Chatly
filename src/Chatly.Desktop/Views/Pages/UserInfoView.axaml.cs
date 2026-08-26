using Avalonia.Controls;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.ViewModels.Pages;

namespace Chatly.Desktop.Views.Pages;

public partial class UserInfoView : UserControl, INavigableView<UserInfoPageViewModel>
{
    public UserInfoView(UserInfoPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public UserInfoPageViewModel ViewModel { get; }
}