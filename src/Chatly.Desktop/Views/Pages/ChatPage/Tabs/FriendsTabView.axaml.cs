using Avalonia;
using Avalonia.Controls;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Tabs;

namespace Chatly.Desktop.Views.Pages.ChatPage.Tabs;

public partial class FriendsTabView : UserControl
{
    public static readonly DirectProperty<FriendsTabView, FriendsTabViewModel?> ViewModelProperty =
        AvaloniaProperty.RegisterDirect<FriendsTabView, FriendsTabViewModel?>(
            nameof(ViewModel),
            control => control.ViewModel,
            (control, value) => control.ViewModel = value);

    private FriendsTabViewModel? _viewModel;

    public FriendsTabView()
    {
        DataContext = this;
        InitializeComponent();
    }

    public FriendsTabViewModel? ViewModel
    {
        get => _viewModel;
        set => SetAndRaise(ViewModelProperty, ref _viewModel, value);
    }
}
