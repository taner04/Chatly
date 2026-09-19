using Avalonia;

namespace Chatly.Desktop.Views.Controls;

public partial class UnreadBadge : UserControl
{
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<UnreadBadge, int>(nameof(Count));

    public UnreadBadge()
    {
        InitializeComponent();
    }

    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }
}