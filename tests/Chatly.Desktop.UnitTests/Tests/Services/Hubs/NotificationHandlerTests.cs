using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Contracts.Features.FriendRequests.Models;
using Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;
using Chatly.Contracts.Features.Users.Notifications;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Models.UserSession;
using Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;
using Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;
using Chatly.Desktop.UnitTests.Infrastructure;

namespace Chatly.Desktop.UnitTests.Tests.Services.Hubs;

public sealed class NotificationHandlerTests
{
    [Fact]
    public Task OnlineStatusChanged_Should_UpdateRegisteredUser_When_FriendComesOnline() => UiThread.RunAsync(async () =>
    {
        var registry = new UserRegistry();
        var user = registry.GetOrAdd(Guid.NewGuid(), "friend", null, false);

        await new OnlineStatusChangedNotificationHandler(registry)
            .HandleAsync(new OnlineStatusChangedNotification(user.Id, true));

        user.IsOnline.Should().BeTrue();
    });

    [Fact]
    public Task UserProfileUpdated_Should_UpdateNameAndPicture_When_FriendChangesProfile() => UiThread.RunAsync(async () =>
    {
        var registry = new UserRegistry();
        var user = registry.GetOrAdd(Guid.NewGuid(), "old", null, true);

        await new UserProfileUpdatedNotificationHandler(registry)
            .HandleAsync(new UserProfileUpdatedNotification(user.Id, "new", "https://pic"));

        user.Username.Should().Be("new");
        user.ProfilePictureUrl.Should().Be("https://pic");
    });

    [Fact]
    public Task IncomingFriendRequest_Should_CountAndAnnounceOnce_When_SameRequestArrivesTwice() => UiThread.RunAsync(async () =>
    {
        var state = new FriendRequestState();
        var toasts = Substitute.For<IToastService>();
        var sound = new FakeNotificationSoundPlayer();
        var handler = new IncomingFriendRequestNotificationHandler(state, toasts, sound);
        var notification = new IncomingFriendRequestNotification(
            new FriendRequestContract(Guid.NewGuid(), Guid.NewGuid(), "sender", null));

        await handler.HandleAsync(notification);
        await handler.HandleAsync(notification);

        state.PendingCount.Should().Be(1);
        state.Items.Should().ContainSingle();
        toasts.ReceivedWithAnyArgs(1).AddToast(default!);
        sound.NotificationCount.Should().Be(1);
    });
}
