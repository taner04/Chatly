using System;
using Avalonia.Controls;
using Avalonia.Input;
using Chatly.Desktop.Abstractions.Popups;
using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.Views.Popups;

public partial class PopupOverlayHost : UserControl, IPopupHost
{
    private IPopupViewModel? _currentPopupViewModel = null!;
    public PopupOverlayHost()
    {
        ViewModel = new PopupOverlayHostViewModel();
        DataContext = ViewModel;

        InitializeComponent();
    }

    public PopupOverlayHostViewModel ViewModel { get; }
    
    public void Show<TViewModel>(IPopupOverlay<TViewModel> overlay) where TViewModel : IPopupViewModel
    {
        ArgumentNullException.ThrowIfNull(overlay);
        
        _currentPopupViewModel = overlay.ViewModel;
        ViewModel.Title = _currentPopupViewModel.Title;
        ViewModel.Content = overlay;   
        ViewModel.IsOpen = true;
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
            _currentPopupViewModel?.CloseOverlay();
        }
    }
}
