using Chatly.Desktop.Abstraction.Navigation;
using Chatly.Desktop.Models;
using Chatly.Desktop.Models.UserSession;
using Chatly.Desktop.ViewModels.Pages.ChatPage;

namespace Chatly.Desktop.UnitTests.Tests.ViewModels.Pages.ChatPage;

public sealed class ChatSidebarViewModelTests
{
    [Fact]
    public void ReceiveIncomingMessage_Should_IncreaseUnreadCount_When_ChatIsKnown()
    {
        var state = new DirectChatState();
        var chat = new DirectChat { Id = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), Username = "friend" } };
        state.Set([chat]);
        using var viewModel = new ChatSidebarViewModel(Substitute.For<INavigationService>(), state);

        viewModel.ReceiveIncomingMessage(chat.Id);
        viewModel.ReceiveIncomingMessage(Guid.NewGuid());

        chat.UnreadMessageCount.Should().Be(1);
    }
}
