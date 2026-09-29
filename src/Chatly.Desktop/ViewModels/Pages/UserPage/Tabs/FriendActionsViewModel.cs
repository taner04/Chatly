using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.Services.Friendships;
using Chatly.Desktop.ViewModels.Pages.ChatPage;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed partial class FriendActionsViewModel(
    INavigationService navigationService,
    FriendsApiClient friendsApiClient,
    FriendshipStateService friendshipStateService,
    IToastService toastService) : ViewModelBase
{
    private static bool CanOpenChat(Friend? friend) => friend?.ChatId is not null;

    [RelayCommand(CanExecute = nameof(CanOpenChat))]
    private async Task OpenChat(Friend? friend, CancellationToken cancellationToken)
    {
        if (friend?.ChatId is { } chatId)
        {
            await navigationService.NavigateToAsync<ChatPageViewModel>(chatId, cancellationToken);
        }
    }

    [RelayCommand]
    private async Task RemoveFriendAsync(Friend? friend, CancellationToken cancellationToken)
    {
        if (friend is null)
        {
            return;
        }

        var result = await friendsApiClient.RemoveFriendshipAsync(
            friend.User.Id,
            cancellationToken);
        if (result.IsFailure)
        {
            toastService.ShowError(result.Error);
            return;
        }

        await friendshipStateService.ApplyRemovedAsync(friend.User.Id);
    }
}