using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

public sealed partial class ChatPreviewViewModel(
    DirectChat chat,
    INavigationService navigationService,
    int unreadMessageCount = 0) : ViewModelBase
{
    public User User { get; } = chat.User;

    public Guid? DirectChatId { get; } = chat.Id;

    public string Name => User.Username ?? "Unknown user";

    public string Preview => "Start a conversation";

    public string Time => string.Empty;

    [ObservableProperty] public partial int UnreadMessageCount { get; set; } = unreadMessageCount;

    [ObservableProperty] public partial bool IsSelected { get; set; }

    public bool HasUnreadMessages => UnreadMessageCount > 0;

    partial void OnUnreadMessageCountChanged(int value)
    {
        OnPropertyChanged(nameof(HasUnreadMessages));
    }

    [RelayCommand]
    private void Navigate()
    {
        UnreadMessageCount = 0;
        navigationService.NavigateTo<ChatPageViewModel>(this);
    }
}