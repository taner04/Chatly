using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

[SingletonService]
public sealed class ChatSidebarViewModel : ViewModelBase, IDisposable
{
    private readonly DirectChatState _directChatState;
    private readonly INavigationService _navigationService;

    public ChatSidebarViewModel(
        INavigationService navigationService,
        DirectChatState directChatState)
    {
        _navigationService = navigationService;
        _directChatState = directChatState;
        Chats = [.. directChatState.Items.Select(CreateChatPreview)];
        UnreadChats = [];

        directChatState.CollectionChanged += DirectChats_CollectionChanged;
        SubscribeToChats();
        RefreshUnreadChats();
    }

    public ObservableCollection<ChatPreviewViewModel> Chats { get; }

    public ObservableCollection<ChatPreviewViewModel> UnreadChats { get; }

    public void Dispose()
    {
        _directChatState.CollectionChanged -= DirectChats_CollectionChanged;

        foreach (var chat in Chats)
        {
            chat.PropertyChanged -= Chat_PropertyChanged;
            chat.Dispose();
        }
    }

    internal void ReceiveIncomingMessage(Guid chatId)
    {
        var chat = _directChatState.Items.FirstOrDefault(directChat => directChat.Id == chatId);
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

        var chatList = chats.ToList();
        var existingPreviews = Chats
            .Where(chat => chat.DirectChatId.HasValue)
            .ToDictionary(chat => chat.DirectChatId!.Value);
        var currentChatIds = chatList.Select(chat => chat.Id).ToHashSet();

        foreach (var chat in Chats)
        {
            chat.PropertyChanged -= Chat_PropertyChanged;
            if (chat.DirectChatId is not { } chatId || !currentChatIds.Contains(chatId))
            {
                chat.Dispose();
            }
        }

        Chats.Clear();
        foreach (var chat in chatList)
        {
            Chats.Add(existingPreviews.GetValueOrDefault(chat.Id) ?? CreateChatPreview(chat));
        }

        SubscribeToChats();
        RefreshUnreadChats();
    }

    private ChatPreviewViewModel CreateChatPreview(DirectChat chat) => new(chat, _navigationService);

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