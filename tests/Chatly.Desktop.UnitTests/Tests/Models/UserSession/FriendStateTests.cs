using Chatly.Desktop.Models;
using Chatly.Desktop.Models.UserSession;

namespace Chatly.Desktop.UnitTests.Tests.Models.UserSession;

public sealed class FriendStateTests
{
    [Fact]
    public void Add_Should_UpdateChatOfExistingFriend_When_FriendIsAddedTwice()
    {
        var state = new FriendState();
        var user = new User { Id = Guid.NewGuid(), Username = "friend" };
        var chatId = Guid.NewGuid();

        state.Add(new Friend { User = user });
        state.Add(new Friend { User = user, ChatId = chatId });

        state.Items.Should().ContainSingle().Which.ChatId.Should().Be(chatId);
    }

    [Fact]
    public void OnlineFriends_Should_FollowUserOnlineStatus_When_StatusChanges()
    {
        var state = new FriendState();
        var user = new User { Id = Guid.NewGuid(), Username = "friend" };
        state.Add(new Friend { User = user });

        user.IsOnline = true;
        var whileOnline = state.OnlineFriends.Count;
        user.IsOnline = false;

        whileOnline.Should().Be(1);
        state.OnlineFriends.Should().BeEmpty();
    }

    [Fact]
    public void Remove_Should_StopTrackingOnlineStatus_When_FriendIsRemoved()
    {
        var state = new FriendState();
        var user = new User { Id = Guid.NewGuid(), IsOnline = true };
        state.Add(new Friend { User = user });

        state.Remove(user.Id).Should().BeTrue();
        user.IsOnline = false;
        user.IsOnline = true;

        state.Items.Should().BeEmpty();
        state.OnlineFriends.Should().BeEmpty();
    }
}
