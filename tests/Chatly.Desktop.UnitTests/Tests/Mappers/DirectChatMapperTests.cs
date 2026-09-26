using Chatly.Contracts.Features.Chats.Endpoints.GetChats;
using Chatly.Contracts.Features.Friendships.Models;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Models.UserSession;

namespace Chatly.Desktop.UnitTests.Tests.Mappers;

public sealed class DirectChatMapperTests
{
    [Fact]
    public void Map_Should_ShareUserInstanceWithFriend_When_BothReferenceSameUser()
    {
        var registry = new UserRegistry();
        var userId = Guid.NewGuid();
        var chat = DirectChatMapper.Map(new GetChatsResponse(Guid.NewGuid(), userId, "friend", null, true, 3), registry);
        var friend = FriendMapper.Map(new FriendshipContract(Guid.NewGuid(), chat.Id, userId, "friend", null, true), registry);

        chat.User.Should().BeSameAs(friend.User);
        chat.UnreadMessageCount.Should().Be(3);
    }
}
