using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

public sealed partial class ChatPreviewViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public ChatPreviewViewModel(
        DirectChat chat,
        INavigationService navigationService,
        int unreadMessageCount = 0)
    {
        _navigationService = navigationService;
        User = chat.User;
        DirectChatId = chat.Id;
        UnreadMessageCount = unreadMessageCount;
        User.PropertyChanged += OnUserPropertyChanged;
    }

    public User User { get; }

    public Guid? DirectChatId { get; }

    public string Name => User.Username ?? "Unknown user";

    public string Preview => "Start a conversation";

    [ObservableProperty] public partial int UnreadMessageCount { get; set; }

    [ObservableProperty] public partial bool IsSelected { get; set; }

    public bool HasUnreadMessages => UnreadMessageCount > 0;

    partial void OnUnreadMessageCountChanged(int value)
    {
        OnPropertyChanged(nameof(HasUnreadMessages));
    }

    [RelayCommand]
    private async Task Navigate(CancellationToken cancellationToken)
    {
        if (DirectChatId is { } chatId)
        {
            await _navigationService.NavigateToAsync<ChatPageViewModel>(chatId, cancellationToken);
        }
    }

    private void OnUserPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(User.Username))
        {
            OnPropertyChanged(nameof(Name));
        }
    }
}