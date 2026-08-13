using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;

namespace Chatly.Desktop.Features.Popup.Views;

public partial class PopupOverlay : UserControl
{
    public static readonly StyledProperty<bool> IsOpenProperty =
           AvaloniaProperty.Register<PopupOverlay, bool>(nameof(IsOpen));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<PopupOverlay, string?>(nameof(Title));

    public static readonly StyledProperty<object?> PopupContentProperty =
        AvaloniaProperty.Register<PopupOverlay, object?>(nameof(PopupContent));

    public static readonly StyledProperty<ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<PopupOverlay, ICommand?>(nameof(CloseCommand));

    public static readonly StyledProperty<bool> IsCloseButtonVisibleProperty =
        AvaloniaProperty.Register<PopupOverlay, bool>(
            nameof(IsCloseButtonVisible),
            true);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? PopupContent
    {
        get => GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }

    public ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public bool IsCloseButtonVisible
    {
        get => GetValue(IsCloseButtonVisibleProperty);
        set => SetValue(IsCloseButtonVisibleProperty, value);
    }

    public PopupOverlay()
    {
        InitializeComponent();
    }
}
