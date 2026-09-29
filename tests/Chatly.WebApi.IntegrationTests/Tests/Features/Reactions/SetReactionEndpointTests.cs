using Chatly.Contracts.Features.Reactions.Endpoints.SetReaction;
using Chatly.Contracts.Features.Reactions.Models;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Reactions;

public sealed class SetReactionEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
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

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(ReactionType.Fire, response.Content!.ReactionType);
        await using var dbContext = GetDbContext();
        Assert.Equal(1, await dbContext.Reactions.CountAsync(CurrentCancellationToken));
    }
}