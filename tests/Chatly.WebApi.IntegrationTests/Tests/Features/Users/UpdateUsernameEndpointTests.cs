using Chatly.Contracts.Features.Users.Endpoints.UpdateUsername;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Users;

public sealed class UpdateUsernameEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task UpdateUsername_Should_Return409InsteadOf500_When_TwoUsersClaimTheSameNameConcurrently()
    {
        var other = await CreateUserAsync("other");

        var responses = await Task.WhenAll(
            CreateAuthenticatedClient().UpdateUsernameAsync(
                new UpdateUsernameRequest("contested"),
                CurrentCancellationToken),
            CreateAuthenticatedClient(other).UpdateUsernameAsync(
                new UpdateUsernameRequest("contested"),
                CurrentCancellationToken));

        responses.Select(response => response.StatusCode).Should()
            .BeEquivalentTo([HttpStatusCode.OK, HttpStatusCode.Conflict]);
    }

    [Fact]
    public async Task UpdateUsername_Should_Return200_When_UsernameIsFree()
    {
        var client = CreateAuthenticatedClient();

        var response =
            await client.UpdateUsernameAsync(new UpdateUsernameRequest("new_name"), CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Username.Should().Be("new_name");

        await using var dbContext = GetDbContext();
        var user = await dbContext.Users.SingleAsync(user => user.Id == CurrentUser.Id, CurrentCancellationToken);
        user.Username.Should().Be("new_name");
    }

    [Fact]
    public async Task UpdateUsername_Should_Return409_When_UsernameIsTaken()
    {
        var other = await CreateUserAsync("taken_name");
        var client = CreateAuthenticatedClient();

        var response =
            await client.UpdateUsernameAsync(new UpdateUsernameRequest(other.Username), CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateUsername_Should_Return400_When_UsernameContainsInvalidCharacters()
    {
        var client = CreateAuthenticatedClient();

        var response =
            await client.UpdateUsernameAsync(new UpdateUsernameRequest("not valid!"), CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}