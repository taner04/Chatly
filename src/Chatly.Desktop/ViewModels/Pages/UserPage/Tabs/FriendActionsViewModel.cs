using System.Linq;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Services.Api;
using Chatly.Desktop.Services.Chat;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed partial class FriendActionsViewModel(
    ChatNavigationService chatNavigationService,
    FriendsWebService friendsWebService,
    UserSessionContext userSessionContext,
    ChatPageViewModel chatPageViewModel,
    IToastService toastService) : ViewModelBase
{
    private static bool CanOpenChat(Friend? friend)
    {
        return friend?.ChatId is not null;
    }

    [RelayCommand(CanExecute = nameof(CanOpenChat))]
    private void OpenChat(Friend? friend)
    {
        if (friend?.ChatId is { } chatId)
        {
            chatNavigationService.Navigate(chatId);
        }
    }

    [RelayCommand]
    private async Task RemoveFriendAsync(Friend? friend)
    {
        if (friend is null)
        {
            return;
        }

        var result = await friendsWebService.RemoveFriendshipAsync(friend.User.Id);
        if (result.IsFailure)
        {
            toastService.ShowError(result.Error);
            return;
        }

        var chatId = friend.ChatId ?? userSessionContext.DirectChats
            .FirstOrDefault(chat => chat.User.Id == friend.User.Id)?.Id;
        if (chatId is not null)
        {
            await chatPageViewModel.CloseChatAsync(chatId.Value);
        }

        userSessionContext.RemoveFriend(friend.User.Id);
    }
}