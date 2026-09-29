using Chatly.Desktop.Models;
using Chatly.Desktop.Models.UserSession;

namespace Chatly.Desktop.UnitTests.Tests.Models.UserSession;

public sealed class UserRegistryTests
{
    [Fact]
    public void GetOrAdd_Should_ReturnSameInstanceWithUpdatedValues_When_UserIsKnown()
    {
        var registry = new UserRegistry();
        var userId = Guid.NewGuid();

        var first = registry.GetOrAdd(userId, "old", null, false);
        var second = registry.GetOrAdd(new User { Id = userId, Username = "new", IsOnline = true });

        second.Should().BeSameAs(first);
        first.Username.Should().Be("new");
        first.IsOnline.Should().BeTrue();
        registry.Find(userId).Should().BeSameAs(first);
    }

    [Fact]
    public void UpdateProfile_Should_IgnoreUnknownUsers_When_UserIsNotRegistered()
    {
        var registry = new UserRegistry();
        var userId = Guid.NewGuid();

        registry.UpdateProfile(userId, "ghost", null);

        registry.Find(userId).Should().BeNull();
    }
}