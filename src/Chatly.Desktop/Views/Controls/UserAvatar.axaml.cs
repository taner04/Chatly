using Avalonia;

namespace Chatly.Desktop.Views.Controls;

public partial class UserAvatar : UserControl
{
    public static readonly StyledProperty<object?> ProfilePictureUrlProperty =
        AvaloniaProperty.Register<UserAvatar, object?>(nameof(ProfilePictureUrl));

    public static readonly StyledProperty<string?> DisplayNameProperty =
        AvaloniaProperty.Register<UserAvatar, string?>(nameof(DisplayName));

    public static readonly StyledProperty<bool> IsOnlineProperty =
        AvaloniaProperty.Register<UserAvatar, bool>(nameof(IsOnline));

    public UserAvatar()
    {
        InitializeComponent();
    }

    public object? ProfilePictureUrl
    {
        get => GetValue(ProfilePictureUrlProperty);
        set => SetValue(ProfilePictureUrlProperty, value);
    }

    public string? DisplayName
    {
        get => GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public bool IsOnline
    {
        get => GetValue(IsOnlineProperty);
        set => SetValue(IsOnlineProperty, value);
    }
}