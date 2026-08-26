using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Chatly.Desktop.Abstractions.Popups;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Popups;

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

}