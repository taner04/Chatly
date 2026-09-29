namespace Chatly.WebApi.IntegrationTests.Tests.Features.Health;

public sealed class CheckStatusEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task CheckStatus_Should_Return200_When_DatabaseAndStorageAreReachable()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.CheckStatusAsync(CurrentCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}