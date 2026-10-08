using System.Globalization;
using System.Web;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Messages;

public sealed class GetMessagesEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task GetMessages_Should_PageFromNewestToOldest_When_MoreMessagesThanPageSize()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();
        foreach (var text in new[] { "first", "second", "third" })
        {
            await client.SendMessageAsync(chatId.Value, text, CurrentCancellationToken);
        }

        var firstPage = await client.GetMessagesAsync(chatId.Value, 2, CurrentCancellationToken);
        var secondPage = await client.GetMessagesBeforeAsync(
            chatId.Value,
            firstPage.Content!.NextBeforeSentAt!.Value,
            firstPage.Content.NextBeforeMessageId!.Value,
            2,
            CurrentCancellationToken);

        firstPage.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPage.Content.HasMore.Should().BeTrue();
        firstPage.Content.Items.Select(message => message.Content).Should().Equal("second", "third");
        secondPage.Content!.HasMore.Should().BeFalse();
        secondPage.Content.Items.Should().ContainSingle().Subject.Content.Should().Be("first");
    }

    [Fact]
    public async Task GetMessages_Should_Return404_When_UserIsNotPartOfTheChat()
    {
        var first = await CreateUserAsync("first");
        var second = await CreateUserAsync("second");
        var chatId = await CreateFriendshipAsync(first, second);

        var response = await CreateAuthenticatedClient().GetMessagesAsync(chatId.Value, 20, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMessages_Should_MarkRemovedMessagesAsDeleted_When_SenderRemovedThem()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();
        var sent = await client.SendMessageAsync(chatId.Value, "secret", CurrentCancellationToken);
        await client.RemoveMessageAsync(sent.Content!.MessageId, CurrentCancellationToken);

        var response = await CreateAuthenticatedClient(friend)
            .GetMessagesAsync(chatId.Value, 20, CurrentCancellationToken);

        var message = response.Content!.Items.Should().ContainSingle().Subject;
        message.IsDeleted.Should().BeTrue();
        message.Content.Should().NotContain("secret");
    }

    [Fact]
    public async Task GetMessages_Should_ReturnStableLongLivedAttachmentUrl_When_LoadedRepeatedly()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();
        await client.SendMessageWithFilesAsync(
            chatId.Value,
            "photo",
            [CreateFile("photo.png", "image/png")],
            CurrentCancellationToken);

        var first = await client.GetMessagesAsync(chatId.Value, 20, CurrentCancellationToken);
        var second = await client.GetMessagesAsync(chatId.Value, 20, CurrentCancellationToken);

        var url = first.Content!.Items.Single().Attachments.Single().Url;
        second.Content!.Items.Single().Attachments.Single().Url.Should().Be(url);
        var expiresOn = DateTimeOffset.Parse(
            HttpUtility.ParseQueryString(new Uri(url).Query)["se"]!,
            CultureInfo.InvariantCulture);
        expiresOn.Should().BeAfter(DateTimeOffset.UtcNow.AddHours(24));
    }
}