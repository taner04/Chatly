using Avalonia;
using Avalonia.Controls;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Tabs;

namespace Chatly.Desktop.Views.Pages.ChatPage.Tabs;

public partial class AllFriendTabPage : UserControl
{
    public static readonly DirectProperty<AllFriendTabPage, AllFriendTabPageViewModel?> ViewModelProperty =
        AvaloniaProperty.RegisterDirect<AllFriendTabPage, AllFriendTabPageViewModel?>(
            nameof(ViewModel),
            control => control.ViewModel,
            (control, value) => control.ViewModel = value);

    private AllFriendTabPageViewModel? _viewModel;

    public AllFriendTabPage()
    {
        DataContext = this;
        InitializeComponent();
    }

    public AllFriendTabPageViewModel? ViewModel
    {
        get => _viewModel;
        set => SetAndRaise(ViewModelProperty, ref _viewModel, value);
    }
}
