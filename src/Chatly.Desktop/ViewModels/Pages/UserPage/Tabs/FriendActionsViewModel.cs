using System.Linq;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Models.UserSession;
using Chatly.Desktop.Services.Api;
using Chatly.Desktop.Services.Chat;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed partial class FriendActionsViewModel(
    ChatNavigationService chatNavigationService,
    FriendsApiClient friendsApiClient,
    FriendState friendState,
    DirectChatState directChatState,
    ChatPageViewModel chatPageViewModel,
    IToastService toastService) : ViewModelBase
{
    private static bool CanOpenChat(Friend? friend)
    {
        return friend?.ChatId is not null;
    }

    [RelayCommand(CanExecute = nameof(CanOpenChat))]
    private async Task OpenChat(Friend? friend, CancellationToken cancellationToken)
    {
        if (friend?.ChatId is { } chatId)
        {
            await chatNavigationService.NavigateAsync(chatId, cancellationToken);
        }
    }

    [RelayCommand]
    private async Task RemoveFriendAsync(Friend? friend)
    {
        if (friend is null)
        {
            return;
        }

        var result = await friendsApiClient.RemoveFriendshipAsync(friend.User.Id);
        if (result.IsFailure)
        {
            toastService.ShowError(result.Error);
            return;
        }

        var chatId = friend.ChatId ?? directChatState.Items
            .FirstOrDefault(chat => chat.User.Id == friend.User.Id)?.Id;
        if (chatId is not null)
        {
            await chatPageViewModel.CloseChatAsync(chatId.Value);
        }

        friendState.Remove(friend.User.Id);
        directChatState.RemoveByUserId(friend.User.Id);
    }
}
