using System.Text.Json;
using Chatly.Contracts.Features.DeviceSessions.Notifications;
using Chatly.Contracts.Features.FriendRequests.Endpoints.AcceptFriendRequest;
using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Contracts.Features.FriendRequests.Models;
using Chatly.Contracts.Features.Friendships.Endpoints.RemoveFriendship;
using Chatly.Contracts.Features.Friendships.Models;
using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;
using Chatly.Contracts.Features.Messages.Endpoints.RemoveMessage;
using Chatly.Contracts.Features.Messages.Endpoints.SendMessage;
using Chatly.Contracts.Features.Messages.Models;
using Chatly.Contracts.Features.Reactions.Models;
using Chatly.Contracts.Features.Reactions.Notifications;
using Chatly.Contracts.Features.Users.Notifications;

namespace Chatly.WebApi.UnitTests.Tests.Contracts.Notifications;

public sealed class NotificationContractSerializationTests
{
    private static readonly Guid Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly DateTimeOffset SentAt = new(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);

    public static TheoryData<NotificationMessage> Notifications =>
    [
        new IncomingFriendRequestNotification(new FriendRequestContract(Id, Id, "sender", null)),
        new FriendRequestAcceptedNotification(new FriendshipContract(Id, Id, Id, "friend", "https://pic", true)),
        new IncomingMessageNotification(new MessageContract(
            Id,
            Id,
            Id,
            "hello",
            SentAt,
            false,
            [new MessageAttachmentContract(Id, "file.txt", "text/plain", 12, "https://file")],
            [new MessageReactionContract(Id, Id, ReactionType.Fire)])),
        new MessageDeletedNotification(Id, Id),
        new ReactionChangedNotification(Id, Id, Id, Id, ReactionType.Thanks, true),
        new TypingStatusChangedNotification(Id, true),
        new OnlineStatusChangedNotification(Id, false),
        new FriendshipRemovedNotification(Id),
        new UserProfileUpdatedNotification(Id, "renamed", null),
        new DeviceSessionRevokedNotification(),
        new DeviceSessionsChangedNotification()
    ];

    [Theory]
    [MemberData(nameof(Notifications))]
    public void Notification_Should_RoundTripPolymorphically_When_SerializedAsBaseType(NotificationMessage expected)
    {
        var json = JsonSerializer.Serialize(expected);

        var actual = JsonSerializer.Deserialize<NotificationMessage>(json);

        json.Should().Contain("\"$notificationType\":");
        actual.Should().BeOfType(expected.GetType());
        JsonSerializer.Serialize(actual).Should().Be(json);
    }

    [Fact]
    public void Every_Notification_Type_Should_BeRegisteredForPolymorphism_When_ContractsChange()
    {
        var registered = typeof(NotificationMessage).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false } && type.IsSubclassOf(typeof(NotificationMessage)))
            .ToList();

        Notifications.Select(row => row.Data.GetType()).Should().BeEquivalentTo(registered);
    }
}