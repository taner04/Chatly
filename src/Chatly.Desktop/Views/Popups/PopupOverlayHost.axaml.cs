using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Chatly.Desktop.Abstractions.Popups;
using Chatly.Desktop.Services.Popups;
using Chatly.Desktop.ViewModels.Popups;
using System;

namespace Chatly.Desktop.Views.Popups;

public partial class PopupOverlayHost : UserControl, IPopupHost
{
    public event EventHandler<PopupOverlayEventArgs>? PopupEvent;

    private IPopupViewModel? _currentPopupViewModel = null!;
    private TopLevel? _topLevel;

    public PopupOverlayHost()
    {
        ViewModel = new PopupOverlayHostViewModel();
        DataContext = this;

        InitializeComponent();
    }

    public PopupOverlayHostViewModel ViewModel { get; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.AddHandler(KeyUpEvent, OnHostKeyUp, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _topLevel?.RemoveHandler(KeyUpEvent, OnHostKeyUp);
        _topLevel = null;

        base.OnDetachedFromVisualTree(e);
    }
    
    public void Show<TViewModel>(IPopupOverlay<TViewModel> overlay) where TViewModel : IPopupViewModel
    {
        ArgumentNullException.ThrowIfNull(overlay);
        
        _currentPopupViewModel = overlay.ViewModel;
        ViewModel.Title = _currentPopupViewModel.Title;
        ViewModel.Content = overlay;   
        ViewModel.IsOpen = true;

        Dispatcher.UIThread.Post(() => Focus());
    }

    public void Close()
    {
        ViewModel.IsOpen = false;
        ViewModel.Title = null;
        ViewModel.Content = null;
        _currentPopupViewModel = null;
    }

    private void OnBackdropPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ReferenceEquals(e.Source, sender))
        {
            PopupEvent?.Invoke(this, new PopupOverlayEventArgs(PopupOverlayHostEventType.MouseEvent, e));
        }
    }

    private void OnHostKeyUp(object? sender, KeyEventArgs e)
    {
        PopupEvent?.Invoke(this, new PopupOverlayEventArgs(PopupOverlayHostEventType.KeyEvent, e));
    }
}
