using Avalonia.Controls;
using Avalonia.Input;
using Chatly.Desktop.Abstractions.Popups;
using Chatly.Desktop.Services.Popups;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Popups;
using Chatly.Desktop.Views.Popups;

namespace Chatly.Desktop.Views.Pages.ChatPage.Popups;

public partial class AddFriendPopupOverlay : UserControl, IPopupOverlay<AddFriendPopupOverlayViewModel>
{
    public AddFriendPopupOverlay(AddFriendPopupOverlayViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
    }
    
    public AddFriendPopupOverlayViewModel ViewModel { get; }

    public void HandlePopupEvent(object? sender, PopupOverlayEventArgs args)
    {
        if(args.Type == PopupOverlayHostEventType.KeyEvent && args.Data is KeyEventArgs keyEventArgs && keyEventArgs.Key == Key.Escape)
        {
            ViewModel.CloseOverlay();
        }
        else if(args.Type == PopupOverlayHostEventType.MouseEvent && args.Data is PointerPressedEventArgs pointerPressedEventArgs)
        {
            // Check if the click was outside the popup
            var clickPosition = pointerPressedEventArgs.GetPosition(this);
            if (!Bounds.Contains(clickPosition))
            {
                ViewModel.CloseOverlay();
            }
        }
        else if(args.Type == PopupOverlayHostEventType.KeyEvent && args.Data is KeyEventArgs keyEventArgs2 && keyEventArgs2.Key == Key.Enter)
        {
            ViewModel.SearchForUsersCommand.Execute(null);
        }
    }
}
