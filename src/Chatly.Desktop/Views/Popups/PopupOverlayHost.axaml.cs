using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Chatly.Desktop.Services.Popups;
using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.Views.Popups;

[SingletonService]
public partial class PopupOverlayHost : UserControl, IPopupHost
{
    private IPopupViewModel? _currentPopupViewModel;
    private TopLevel? _topLevel;

    public PopupOverlayHost(PopupOverlayHostViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public PopupOverlayHostViewModel ViewModel { get; }
    public event EventHandler<PopupOverlayEventArgs>? PopupEvent;

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