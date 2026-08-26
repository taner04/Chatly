using System.Collections.Generic;
using System.Threading.Tasks;
using Chatly.Desktop.Abstractions.Popups;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Popups;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage.Tabs;

public sealed partial class FriendsTabViewModel(IPopupService popupService) : ViewModelBase
{
    public string Title { get; init; } = string.Empty;

    public List<string> Friends { get; init; } = [];

    public bool ShowSearch { get; init; }

    [RelayCommand]
    private async Task AddFriend()
    {
        await popupService.ShowAsync<AddFriendPopupOverlayViewModel>();
    }
}
