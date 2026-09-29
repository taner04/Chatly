using Chatly.Desktop.Models.UserSession;

namespace Chatly.Desktop.UnitTests.Tests.Models.UserSession;

public sealed class FriendRequestStateTests
{
    [Fact]
    public void DecrementPendingCount_Should_NeverGoBelowZero_When_CountIsZero()
    {
        var state = new FriendRequestState();

        state.DecrementPendingCount();

        state.PendingCount.Should().Be(0);
        state.HasPendingRequests.Should().BeFalse();
    }

    [Fact]
    public void Clear_Should_ResetPendingCount_When_SessionEnds()
    {
        var state = new FriendRequestState();
        state.SetPendingCount(3);
        state.IncrementPendingCount();

        state.Clear();

        state.PendingCount.Should().Be(0);
    }
}