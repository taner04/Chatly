using Avalonia;
using Avalonia.Interactivity;
using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

namespace Chatly.Desktop.Views.Pages.UserPage.Tabs;

public partial class FriendsTabView : UserControl
{
    public static readonly StyledProperty<bool> ShowCallButtonProperty =
        AvaloniaProperty.Register<FriendsTabView, bool>(nameof(ShowCallButton));

    public static readonly DirectProperty<FriendsTabView, FriendsTabViewModel?> ViewModelProperty =
        AvaloniaProperty.RegisterDirect<FriendsTabView, FriendsTabViewModel?>(
            nameof(ViewModel),
            control => control.ViewModel,
            (control, value) => control.ViewModel = value);

    public FriendsTabView()
    {
        InitializeComponent();
    }

    public FriendsTabViewModel? ViewModel
    {
        get;
        set => SetAndRaise(ViewModelProperty, ref field, value);
    }

    public bool ShowCallButton
    {
        get => GetValue(ShowCallButtonProperty);
        set => SetValue(ShowCallButtonProperty, value);
    }

    private void RemoveFriend_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: Friend friend } || ViewModel is null)
        {
            return;
        }

        var command = ViewModel.Actions.RemoveFriendCommand;
        if (command.CanExecute(friend))
        {
            command.Execute(friend);
        }
    }
}
