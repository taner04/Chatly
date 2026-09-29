using Chatly.Contracts.Features.FriendRequests.Models;
using Chatly.Desktop.Mappers;

namespace Chatly.Desktop.UnitTests.Tests.Mappers;

public sealed class FriendRequestMapperTests
{
    [Fact]
    public void Map_Should_CopySenderData_When_RequestIsMapped()
    {
        var contract = new FriendRequestContract(Guid.NewGuid(), Guid.NewGuid(), "sender", "https://pic");

        var request = FriendRequestMapper.Map(contract);

        request.Id.Should().Be(contract.FriendRequestId);
        request.SenderUsername.Should().Be("sender");
        request.ProfilePictureUrl.Should().Be("https://pic");
    }
}