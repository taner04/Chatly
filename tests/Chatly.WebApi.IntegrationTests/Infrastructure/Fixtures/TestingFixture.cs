using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Chatly.WebApi.Common.Infrastructure.Email;
using Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Jwt;
using Chatly.WebApi.IntegrationTests.Infrastructure.TestContainers.Azurite;
using Chatly.WebApi.IntegrationTests.Infrastructure.TestContainers.Postgres;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Refit;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Fixtures;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class TestingFixture : IAsyncLifetime
{
    private readonly AzuriteContainer _azurite = new();
    private readonly PostgresTestDatabase _database = new();
    private WebApiFactory? _factoryInstance;

    private WebApiFactory _factory =>
        _factoryInstance ?? throw new InvalidOperationException("The test API has not been started.");

    internal IEmailService EmailService => _factory.EmailService;

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_database.InitializeAsync(), _azurite.InitializeAsync().AsTask());

        _factoryInstance = new WebApiFactory(_database.ConnectionString, _azurite.ConnectionString);
        _ = _factoryInstance.Server;
    }

    public async ValueTask DisposeAsync()
    {
        if (_factoryInstance is not null)
        {
            await _factoryInstance.DisposeAsync();
        }

        await _azurite.DisposeAsync();
        await _database.DisposeAsync();
    }

    internal async Task ResetAsync()
    {
        await _database.ResetAsync();
        EmailService.ClearSubstitute(ClearOptions.All);
    }

    internal ChatlyDbContext CreateDbContext() => _database.CreateDbContext();

    internal IServiceScope CreateScope() => _factory.Services.CreateScope();

    internal HttpClient CreateHttpClient() => _factory.CreateClient();

    internal IChatlyApiClient CreateApiClient(TestUser? user)
    {
        var client = _factory.CreateClient();
        if (user is not null)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", JwtTokenMock.CreateToken(user));
        }

        return RestService.For<IChatlyApiClient>(client);
    }

    internal Task<CallHubTestClient> ConnectCallHubAsync(TestUser user) =>
        ConnectHubAsync(ApiRoutes.Hubs.Call, user, connection => new CallHubTestClient(connection));

    internal Task<NotificationHubTestClient> ConnectNotificationHubAsync(TestUser user) =>
        ConnectHubAsync(ApiRoutes.Hubs.Notification, user, connection => new NotificationHubTestClient(connection));

    private async Task<TClient> ConnectHubAsync<TClient>(
        string route,
        TestUser user,
        Func<HubConnection, TClient> createClient)
    {
        var token = JwtTokenMock.CreateToken(user);
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, route), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .Build();

        var client = createClient(connection);
        await connection.StartAsync(TestContext.Current.CancellationToken);
        return client;
    }
}
