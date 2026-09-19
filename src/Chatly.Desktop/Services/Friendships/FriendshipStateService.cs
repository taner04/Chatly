using Chatly.Contracts.Features.Friendships.Models;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.ViewModels.Pages.ChatPage;

namespace Chatly.Desktop.Services.Friendships;

[SingletonService]
public sealed class FriendshipStateService(
    UserRegistry userRegistry,
    FriendState friendState,
    DirectChatState directChatState,
    ChatPageViewModel chatPageViewModel)
{
    internal void ApplyAccepted(FriendshipContract friendship)
    {
        friendState.Add(FriendMapper.Map(friendship, userRegistry));
        directChatState.Add(DirectChatMapper.Map(friendship, userRegistry));
    }

    internal async Task<string?> ApplyRemovedAsync(Guid userId)
    {
        var friend = friendState.Items.FirstOrDefault(existing => existing.User.Id == userId);
        var directChat = directChatState.Items.FirstOrDefault(chat => chat.User.Id == userId);
        var username = friend?.User.Username ?? directChat?.User.Username;
        var chatId = directChat?.Id ?? friend?.ChatId;

        if (chatId is not null)
        {
            await chatPageViewModel.CloseChatAsync(chatId.Value);
        }

        friendState.Remove(userId);
        directChatState.RemoveByUserId(userId);
        return username;
    }
}