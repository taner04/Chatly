using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Security.Claims;
using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Common.Infrastructure.Email;
using Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.DeviceSession;
using Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Identity;
using Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Jwt;
using Chatly.WebApi.IntegrationTests.Infrastructure.TestContainers.Azurite;
using Chatly.WebApi.IntegrationTests.Infrastructure.TestContainers.Postgres;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using NSubstitute.ClearExtensions;
using Refit;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Fixtures;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class TestingFixture : IAsyncLifetime
{
    private readonly AzuriteContainer _azurite = new();
    private readonly PostgresTestDatabase _database = new();
    private WebApiFactory? _factoryInstance;

    private WebApiFactory Factory =>
        _factoryInstance ?? throw new InvalidOperationException("The test API has not been started.");

    internal IEmailService EmailService => Factory.EmailService;

    internal IdentityProviderHandlerMock IdentityProvider => Factory.IdentityProvider;

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
        EmailService.ClearSubstitute();
        IdentityProvider.Reset();
    }

    internal ChatlyDbContext CreateDbContext() => _database.CreateDbContext();

    internal IServiceScope CreateScope() => Factory.Services.CreateScope();

    internal HttpClient CreateHttpClient() => Factory.CreateClient();

    internal IChatlyApiClient CreateApiClient(TestUser? user, Guid? deviceId = null, string? identitySessionId = null)
    {
        var client = Factory.CreateClient();
        if (user is null)
        {
            return RestService.For<IChatlyApiClient>(client);
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken(user, identitySessionId));
        DeviceSessionHeadersMock.Apply(client.DefaultRequestHeaders, deviceId ?? Guid.NewGuid());

        return RestService.For<IChatlyApiClient>(client);
    }

    internal Task<CallHubTestClient> ConnectCallHubAsync(TestUser user, Guid? deviceId = null) =>
        ConnectHubAsync(ApiRoutes.Hubs.Call, user, deviceId, null, connection => new CallHubTestClient(connection));

    internal Task<NotificationHubTestClient> ConnectNotificationHubAsync(
        TestUser user,
        Guid? deviceId = null,
        string? identitySessionId = null) =>
        ConnectHubAsync(
            ApiRoutes.Hubs.Notification,
            user,
            deviceId,
            identitySessionId,
            connection => new NotificationHubTestClient(connection));

    private static string CreateToken(TestUser user, string? identitySessionId) =>
        JwtTokenMock.CreateToken(
            user,
            identitySessionId is null ? [] : [new Claim(CurrentUserService.SessionIdClaim, identitySessionId)]);

    private async Task<TClient> ConnectHubAsync<TClient>(
        string route,
        TestUser user,
        Guid? deviceId,
        string? identitySessionId,
        Func<HubConnection, TClient> createClient)
    {
        var token = CreateToken(user, identitySessionId);
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(Factory.Server.BaseAddress, route), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                foreach (var (name, value) in DeviceSessionHeadersMock.Create(deviceId ?? Guid.NewGuid()))
                {
                    options.Headers[name] = value;
                }
            })
            .Build();

        var client = createClient(connection);
        await connection.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilServerConnectedAsync(connection);
        return client;
    }

    private static async Task WaitUntilServerConnectedAsync(HubConnection connection)
    {
        try
        {
            await connection.InvokeAsync("ServerConnectedBarrier", TestContext.Current.CancellationToken);
        }
        catch (Exception) when (!TestContext.Current.CancellationToken.IsCancellationRequested)
        {
        }
    }
}