using Chatly.Contracts.Features.Friendships.Models;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Models.UserSession;

namespace Chatly.Desktop.UnitTests.Tests.Mappers;

public sealed class FriendMapperTests
{
    [Fact]
    public void Map_Should_ReuseRegisteredUser_When_UserIsAlreadyKnown()
    {
        var registry = new UserRegistry();
        var friendId = Guid.NewGuid();
        var known = registry.GetOrAdd(friendId, "friend", null, false);
        var contract = new FriendshipContract(Guid.NewGuid(), Guid.NewGuid(), friendId, "friend_renamed", "https://pic", true);

        var friend = FriendMapper.Map(contract, registry);

        friend.User.Should().BeSameAs(known);
        friend.ChatId.Should().Be(contract.DirectChatId);
        known.Username.Should().Be("friend_renamed");
        known.ProfilePictureUrl.Should().Be("https://pic");
        known.IsOnline.Should().BeTrue();
    }
}
