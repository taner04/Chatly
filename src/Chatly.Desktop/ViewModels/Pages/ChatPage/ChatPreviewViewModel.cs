using System.ComponentModel;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

public sealed partial class ChatPreviewViewModel : ViewModelBase, IDisposable
{
    private readonly DirectChat _chat;
    private readonly INavigationService _navigationService;

    internal ChatPreviewViewModel(
        DirectChat chat,
        INavigationService navigationService)
    {
        _chat = chat;
        _navigationService = navigationService;
        User = chat.User;
        DirectChatId = chat.Id;
        _chat.PropertyChanged += OnChatPropertyChanged;
        User.PropertyChanged += OnUserPropertyChanged;
    }

    public User User { get; }

    public Guid? DirectChatId { get; }

    public string Name => User.Username ?? "Unknown user";

    public string Preview => "Start a conversation";

    public int UnreadMessageCount
    {
        get => _chat.UnreadMessageCount;
        set => _chat.UnreadMessageCount = value;
    }

    [ObservableProperty] public partial bool IsSelected { get; set; }

    public bool HasUnreadMessages => UnreadMessageCount > 0;

    public void Dispose()
    {
        _chat.PropertyChanged -= OnChatPropertyChanged;
        User.PropertyChanged -= OnUserPropertyChanged;
    }

    [RelayCommand]
    private async Task Navigate(CancellationToken cancellationToken)
    {
        if (DirectChatId is { } chatId)
        {
            await _navigationService.NavigateToAsync<ChatPageViewModel>(chatId, cancellationToken);
        }
    }

    private void OnChatPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DirectChat.UnreadMessageCount))
        {
            OnPropertyChanged(nameof(UnreadMessageCount));
            OnPropertyChanged(nameof(HasUnreadMessages));
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