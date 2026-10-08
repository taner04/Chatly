using Chatly.Contracts.Features.Reactions.Endpoints.SetReaction;
using Chatly.Contracts.Features.Reactions.Models;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Reactions;

public sealed class SetReactionEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task SetReaction_Should_SucceedForEveryRequest_When_FirstReactionsRunConcurrently()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var sent = await CreateAuthenticatedClient(friend)
            .SendMessageAsync(chatId.Value, "news", CurrentCancellationToken);
        var client = CreateAuthenticatedClient();

        var responses = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(_ => client.SetReactionAsync(
                sent.Content!.MessageId,
                new SetReactionRequest(ReactionType.Like),
                CurrentCancellationToken)));

        responses.Should().AllSatisfy(response => response.StatusCode.Should().Be(HttpStatusCode.OK));
        await using var dbContext = GetDbContext();
        (await dbContext.Reactions.CountAsync(CurrentCancellationToken)).Should().Be(1);
    }

    [Fact]
    public async Task SetReaction_Should_ReplacePreviousReaction_When_UserReactsTwice()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var sent = await CreateAuthenticatedClient(friend)
            .SendMessageAsync(chatId.Value, "news", CurrentCancellationToken);
        var client = CreateAuthenticatedClient();

        await client.SetReactionAsync(sent.Content!.MessageId, new SetReactionRequest(ReactionType.Like),
            CurrentCancellationToken);
        var response = await client.SetReactionAsync(
            sent.Content.MessageId,
            new SetReactionRequest(ReactionType.Fire),
            CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.ReactionType.Should().Be(ReactionType.Fire);
        await using var dbContext = GetDbContext();
        (await dbContext.Reactions.CountAsync(CurrentCancellationToken)).Should().Be(1);
    }
}