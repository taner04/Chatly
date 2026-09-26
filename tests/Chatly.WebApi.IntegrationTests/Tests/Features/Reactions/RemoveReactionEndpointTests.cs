using Chatly.Contracts.Features.Reactions.Endpoints.SetReaction;
using Chatly.Contracts.Features.Reactions.Models;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Reactions;

public sealed class RemoveReactionEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task RemoveReaction_Should_Return204AndDeleteReaction_When_OwnerRemovesIt()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var sent = await CreateAuthenticatedClient(friend).SendMessageAsync(chatId.Value, "news", CurrentCancellationToken);
        var client = CreateAuthenticatedClient();
        var reaction = await client.SetReactionAsync(
            sent.Content!.MessageId,
            new SetReactionRequest(ReactionType.Love),
            CurrentCancellationToken);

        var response = await client.RemoveReactionAsync(reaction.Content!.ReactionId, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await using var dbContext = GetDbContext();
        Assert.False(await dbContext.Reactions.AnyAsync(CurrentCancellationToken));
    }

    [Fact]
    public async Task RemoveReaction_Should_Return404_When_OtherUserTriesToRemoveIt()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var sent = await CreateAuthenticatedClient(friend).SendMessageAsync(chatId.Value, "news", CurrentCancellationToken);
        var reaction = await CreateAuthenticatedClient().SetReactionAsync(
            sent.Content!.MessageId,
            new SetReactionRequest(ReactionType.Clap),
            CurrentCancellationToken);

        var response = await CreateAuthenticatedClient(friend).RemoveReactionAsync(
            reaction.Content!.ReactionId,
            CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await using var dbContext = GetDbContext();
        Assert.True(await dbContext.Reactions.AnyAsync(CurrentCancellationToken));
    }
}
