using Chatly.Desktop.ViewModels.Pages.ChatPage;

namespace Chatly.Desktop.Services.Chat;

[SingletonService]
public sealed class ChatNavigationService(INavigationService navigationService)
{
    internal Task<bool> NavigateAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        return navigationService.NavigateToAsync<ChatPageViewModel>(chatId, cancellationToken);
    }
}
