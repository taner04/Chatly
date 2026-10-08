using Chatly.Desktop.Models;
using Chatly.Desktop.Models.UserSession;

namespace Chatly.Desktop.UnitTests.Tests.Models.UserSession;

public sealed class DirectChatStateTests
{
    [Fact]
    public void RemoveByUserId_Should_RemoveOnlyThatUsersChat_When_SeveralChatsExist()
    {
        var state = new DirectChatState();
        var keep = new DirectChat { Id = Guid.NewGuid(), User = new User { Id = Guid.NewGuid() } };
        var remove = new DirectChat { Id = Guid.NewGuid(), User = new User { Id = Guid.NewGuid() } };
        state.Set([keep, remove]);

        state.RemoveByUserId(remove.User.Id);

        state.Items.Should().ContainSingle().Which.Should().BeSameAs(keep);
    }

    [Fact]
    public void Set_Should_ReplaceAllChats_When_CalledAgain()
    {
        var state = new DirectChatState();
        state.Add(new DirectChat { Id = Guid.NewGuid(), User = new User { Id = Guid.NewGuid() } });
        var replacement = new DirectChat { Id = Guid.NewGuid(), User = new User { Id = Guid.NewGuid() } };

        state.Set([replacement]);

        state.Items.Should().ContainSingle().Which.Should().BeSameAs(replacement);
    }

    [Fact]
    public void Set_Should_UpdateExistingChatInPlaceAndFollowNewOrder_When_ChatsAreReloaded()
    {
        var state = new DirectChatState();
        var first = new DirectChat { Id = Guid.NewGuid(), User = new User { Id = Guid.NewGuid() } };
        var second = new DirectChat { Id = Guid.NewGuid(), User = new User { Id = Guid.NewGuid() } };
        state.Set([first, second]);
        var added = new DirectChat { Id = Guid.NewGuid(), User = new User { Id = Guid.NewGuid() } };

        state.Set(
        [
            added,
            new DirectChat { Id = second.Id, User = second.User, UnreadMessageCount = 3 },
            new DirectChat { Id = first.Id, User = first.User }
        ]);

        state.Items.Should().Equal(added, second, first);
        second.UnreadMessageCount.Should().Be(3);
    }
}