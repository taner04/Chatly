using Chatly.Contracts.Features.Users.Endpoints.UpdateUsername;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Users;

public sealed class UpdateUsernameEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task UpdateUsername_Should_Return200_When_UsernameIsFree()
    {
        var client = CreateAuthenticatedClient();

        var response =
            await client.UpdateUsernameAsync(new UpdateUsernameRequest("new_name"), CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("new_name", response.Content!.Username);

        await using var dbContext = GetDbContext();
        var user = await dbContext.Users.SingleAsync(user => user.Id == CurrentUser.Id, CurrentCancellationToken);
        Assert.Equal("new_name", user.Username);
    }

    [Fact]
    public async Task UpdateUsername_Should_Return409_When_UsernameIsTaken()
    {
        var other = await CreateUserAsync("taken_name");
        var client = CreateAuthenticatedClient();

        var response =
            await client.UpdateUsernameAsync(new UpdateUsernameRequest(other.Username), CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUsername_Should_Return400_When_UsernameContainsInvalidCharacters()
    {
        var client = CreateAuthenticatedClient();

        var response =
            await client.UpdateUsernameAsync(new UpdateUsernameRequest("not valid!"), CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}