using Chatly.Desktop.ViewModels.Pages.ChatPage;

namespace Chatly.Desktop.Services.Chat;

[SingletonService]
public sealed class ChatNavigationService(INavigationService navigationService)
{
    public bool Navigate(Guid chatId)
    {
        return navigationService.NavigateTo<ChatPageViewModel>(chatId);
    }
}