using System.Text.Json;
using Chatly.WebApi.Common.Infrastructure.Email;
using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.DeviceSessions.Models;
using Chatly.WebApi.Features.Friendships.Models;
using Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Identity;
using Refit;

namespace Chatly.WebApi.IntegrationTests.Infrastructure;

[Collection(nameof(TestingFixtureCollection))]
public abstract class TestingBase(TestingFixture fixture) : IAsyncLifetime
{
    private static readonly HttpClient BlobClient = new();

    protected TestUser CurrentUser { get; private set; } = null!;

    protected static CancellationToken CurrentCancellationToken => TestContext.Current.CancellationToken;

    private protected IEmailService EmailService => fixture.EmailService;

    private protected IdentityProviderHandlerMock IdentityProvider => fixture.IdentityProvider;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        CurrentUser = await CreateUserAsync(UserFactory.DefaultUsername);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    protected ChatlyDbContext GetDbContext() => fixture.CreateDbContext();

    protected IServiceScope CreateScope() => fixture.CreateScope();

    protected IChatlyApiClient CreateAuthenticatedClient(
        TestUser? user = null,
        Guid? deviceId = null,
        string? identitySessionId = null) =>
        fixture.CreateApiClient(user ?? CurrentUser, deviceId, identitySessionId);

    protected IChatlyApiClient CreateUnauthenticatedClient() => fixture.CreateApiClient(null);

    protected HttpClient CreateHttpClient() => fixture.CreateHttpClient();

    protected Task<CallHubTestClient> ConnectCallHubAsync(TestUser? user = null, Guid? deviceId = null) =>
        fixture.ConnectCallHubAsync(user ?? CurrentUser, deviceId);

    protected Task<NotificationHubTestClient> ConnectNotificationHubAsync(
        TestUser? user = null,
        Guid? deviceId = null,
        string? identitySessionId = null) =>
        fixture.ConnectNotificationHubAsync(user ?? CurrentUser, deviceId, identitySessionId);

    protected static StreamPart CreateFile(string fileName, string contentType, int size = 64) =>
        new(new MemoryStream(Enumerable.Repeat((byte)1, size).ToArray()), fileName, contentType);

    protected static void AssertError(IApiResponse response, HttpStatusCode statusCode, string errorCode)
    {
        response.StatusCode.Should().Be(statusCode);
        using var problem = JsonDocument.Parse(response.Error.Should().BeAssignableTo<ApiException>().Subject.Content!);
        problem.RootElement.GetProperty("errorCode").GetString().Should().Be(errorCode);
    }

    protected static async Task<HttpStatusCode> GetBlobStatusAsync(string url)
    {
        using var response = await BlobClient.GetAsync(url, CurrentCancellationToken);
        return response.StatusCode;
    }

    protected static async Task<T?> WaitForAsync<T>(Func<Task<T?>> probe)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
        while (true)
        {
            var result = await probe();
            if (result is not null || DateTime.UtcNow >= deadline)
            {
                return result;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), CurrentCancellationToken);
        }
    }

    protected async Task<TestUser> CreateUserAsync(string username)
    {
        await using var context = GetDbContext();
        var user = UserFactory.Create(username);
        context.Users.Add(user);
        await context.SaveChangesAsync(CurrentCancellationToken);
        return UserFactory.ToTestUser(user);
    }

    protected async Task<DeviceSessionId> CreateDeviceSessionAsync(
        TestUser user,
        Guid deviceId,
        string deviceName,
        DateTimeOffset? revokedAt = null,
        string? identitySessionId = null)
    {
        await using var context = GetDbContext();
        var session = new DeviceSession(user.Id, deviceId, deviceName, "tests", "1.0.0")
        {
            RevokedAt = revokedAt,
            IdentitySessionId = identitySessionId
        };
        context.DeviceSessions.Add(session);
        await context.SaveChangesAsync(CurrentCancellationToken);
        return session.Id;
    }

    protected async Task<ChatId> CreateFriendshipAsync(TestUser first, TestUser second)
    {
        await using var context = GetDbContext();
        var chat = new Chat(first.Id, second.Id);
        context.Friendships.Add(new Friendship(first.Id, second.Id));
        context.Chats.Add(chat);
        await context.SaveChangesAsync(CurrentCancellationToken);
        return chat.Id;
    }
}