using System.ComponentModel;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.Views.Popups;

[SingletonService]
internal partial class PopupOverlayHost : UserControl
{
    private TopLevel? _topLevel;

    public PopupOverlayHost(PopupOverlayHostViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        ViewModel.PropertyChanged += ViewModel_OnPropertyChanged;
    }

    public PopupOverlayHostViewModel ViewModel { get; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.AddHandler(KeyUpEvent, OnHostKeyUp, RoutingStrategies.Tunnel, true);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _topLevel?.RemoveHandler(KeyUpEvent, OnHostKeyUp);
        _topLevel = null;

        base.OnDetachedFromVisualTree(e);
    }

    private void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PopupOverlayHostViewModel.Current) && ViewModel.IsOpen)
        {
            Dispatcher.UIThread.Post(() => Focus());
        }
    }

    private void OnBackdropPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ReferenceEquals(e.Source, sender))
        {
            CloseIfDismissible();
        }
    }

    private void OnHostKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CloseIfDismissible();
        }
    }

    private void CloseIfDismissible()
    {
        if (ViewModel.Current is { IsDismissible: true } popup)
        {
            popup.CloseOverlay();
        }
    }
}