using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using UserSessionContext = Chatly.Desktop.Models.UserSession.UserSessionContext;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

[SingletonService]
public sealed class ChatSidebarViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public ChatSidebarViewModel(
        INavigationService navigationService,
        UserSessionContext userSessionContext)
    {
        _navigationService = navigationService;
        Chats = [.. userSessionContext.DirectChats.Select(CreateChatPreview)];
        UnreadChats = [];

        userSessionContext.DirectChats.CollectionChanged += DirectChats_CollectionChanged;
        SubscribeToChats();
        RefreshUnreadChats();
    }

    public ObservableCollection<ChatPreviewViewModel> Chats { get; }

    public ObservableCollection<ChatPreviewViewModel> UnreadChats { get; }

    public void ReceiveIncomingMessage(Guid chatId)
    {
        var chat = Chats.FirstOrDefault(preview => preview.DirectChatId == chatId);
        if (chat is not null)
        {
            chat.UnreadMessageCount++;
        }
    }

    private void DirectChats_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not IEnumerable<DirectChat> chats)
        {
            return;
        }

        var existingPreviews = Chats
            .Where(chat => chat.DirectChatId.HasValue)
            .ToDictionary(chat => chat.DirectChatId!.Value);

        foreach (var chat in Chats)
        {
            chat.PropertyChanged -= Chat_PropertyChanged;
        }

        Chats.Clear();
        foreach (var chat in chats)
        {
            Chats.Add(existingPreviews.GetValueOrDefault(chat.Id) ?? CreateChatPreview(chat));
        }

        SubscribeToChats();
        RefreshUnreadChats();
    }

    private ChatPreviewViewModel CreateChatPreview(DirectChat chat)
    {
        return new ChatPreviewViewModel(chat, _navigationService, chat.UnreadMessageCount);
    }

    private void SubscribeToChats()
    {
        foreach (var chat in Chats)
        {
            chat.PropertyChanged -= Chat_PropertyChanged;
            chat.PropertyChanged += Chat_PropertyChanged;
        }
    }

    private void Chat_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChatPreviewViewModel.UnreadMessageCount))
        {
            RefreshUnreadChats();
        }
    }

    private void RefreshUnreadChats()
    {
        UnreadChats.Clear();

        foreach (var chat in Chats.Where(chat => chat.HasUnreadMessages))
        {
            UnreadChats.Add(chat);
        }
    }
}