using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Contracts.Features.Users.Endpoints.SearchUsers;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Users;

public sealed class SearchUsersEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task SearchUsers_Should_MatchCaseInsensitiveAndExcludeCurrentUser_When_NameMatches()
    {
        await CreateUserAsync("Alice_Test");
        await CreateUserAsync("bob");

        var response = await CreateAuthenticatedClient().SearchUsersAsync("alice", 1, 10, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Alice_Test", Assert.Single(response.Content!.Items).Username);

        var self = await CreateAuthenticatedClient().SearchUsersAsync("current", 1, 10, CurrentCancellationToken);
        Assert.Empty(self.Content!.Items);
    }

    [Fact]
    public async Task SearchUsers_Should_ReportRelationshipStatus_When_UsersAreRelated()
    {
        var friend = await CreateUserAsync("match_friend");
        var outgoing = await CreateUserAsync("match_outgoing");
        var incoming = await CreateUserAsync("match_incoming");
        await CreateUserAsync("match_none");
        await CreateFriendshipAsync(CurrentUser, friend);
        var client = CreateAuthenticatedClient();
        await client.SendFriendRequestAsync(new SendFriendRequestRequest(outgoing.Id.Value), CurrentCancellationToken);
        await CreateAuthenticatedClient(incoming).SendFriendRequestAsync(
            new SendFriendRequestRequest(CurrentUser.Id.Value),
            CurrentCancellationToken);

        var response = await client.SearchUsersAsync("match", 1, 10, CurrentCancellationToken);

        var statuses = response.Content!.Items.ToDictionary(user => user.Username, user => user.RelationshipStatus);
        Assert.Equal(UserRelationshipStatus.Friends, statuses["match_friend"]);
        Assert.Equal(UserRelationshipStatus.OutgoingFriendRequest, statuses["match_outgoing"]);
        Assert.Equal(UserRelationshipStatus.IncomingFriendRequest, statuses["match_incoming"]);
        Assert.Equal(UserRelationshipStatus.None, statuses["match_none"]);
    }

    [Theory]
    [InlineData("_")]
    [InlineData("%")]
    public async Task SearchUsers_Should_TreatWildcardsLiterally_When_SearchContainsThem(string search)
    {
        await CreateUserAsync("under_score");
        await CreateUserAsync("plain");

        var response = await CreateAuthenticatedClient().SearchUsersAsync(search, 1, 10, CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expected = search == "_" ? ["under_score"] : Array.Empty<string>();
        Assert.Equal(expected, response.Content!.Items.Select(user => user.Username));
    }
}