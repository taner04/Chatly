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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Items.Should().ContainSingle().Subject.Username.Should().Be("Alice_Test");

        var self = await CreateAuthenticatedClient().SearchUsersAsync("current", 1, 10, CurrentCancellationToken);
        self.Content!.Items.Should().BeEmpty();
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
        statuses["match_friend"].Should().Be(UserRelationshipStatus.Friends);
        statuses["match_outgoing"].Should().Be(UserRelationshipStatus.OutgoingFriendRequest);
        statuses["match_incoming"].Should().Be(UserRelationshipStatus.IncomingFriendRequest);
        statuses["match_none"].Should().Be(UserRelationshipStatus.None);
    }

    [Theory]
    [InlineData("_")]
    [InlineData("%")]
    public async Task SearchUsers_Should_TreatWildcardsLiterally_When_SearchContainsThem(string search)
    {
        await CreateUserAsync("under_score");
        await CreateUserAsync("plain");

        var response = await CreateAuthenticatedClient().SearchUsersAsync(search, 1, 10, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var expected = search == "_" ? ["under_score"] : Array.Empty<string>();
        response.Content!.Items.Select(user => user.Username).Should().Equal(expected);
    }
}