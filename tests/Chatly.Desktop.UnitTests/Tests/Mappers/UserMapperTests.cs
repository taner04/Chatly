using Chatly.Contracts.Features.Users.Endpoints.GetCurrentUser;
using Chatly.Desktop.Mappers;

namespace Chatly.Desktop.UnitTests.Tests.Mappers;

public sealed class UserMapperTests
{
    [Fact]
    public void Map_Should_CopyCurrentUser_When_ResponseIsMapped()
    {
        var response = new CurrentUserResponse(Guid.NewGuid(), "me@chatly.tests", "me", null, true);

        var user = UserMapper.Map(response);

        user.Id.Should().Be(response.UserId);
        user.Email.Should().Be(response.Email);
        user.OnboardingCompleted.Should().BeTrue();
    }
}
