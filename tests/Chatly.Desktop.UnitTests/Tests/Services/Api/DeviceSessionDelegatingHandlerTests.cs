using System.Net;
using System.Net.Http.Json;
using Chatly.Contracts.Common;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Models.UserSession;
using Chatly.Desktop.Services.Api.Refit.DelegatingHandlers;
using Chatly.Desktop.UnitTests.Infrastructure;
using Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;

namespace Chatly.Desktop.UnitTests.Tests.Services.Api;

public sealed class DeviceSessionDelegatingHandlerTests : TestBase
{
    private readonly AppSettings _appSettings = CallSessionFactory.CreateAppSettings();
    private readonly UserSessionContext _sessionContext = new(new UserRegistry());

    [Fact]
    public async Task SendAsync_Should_AddDeviceHeaders_When_DeviceIdExists()
    {
        var deviceId = Guid.NewGuid();
        _appSettings.DeviceSettings.DeviceId = deviceId;
        var inner = new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK));

        await SendAsync(inner);

        var headers = inner.Request!.Headers;
        headers.GetValues(DeviceSessionHeaders.DeviceId).Should().Equal(deviceId.ToString());
        headers.GetValues(DeviceSessionHeaders.DeviceName).Should().Equal(_appSettings.DeviceName);
        headers.GetValues(DeviceSessionHeaders.Platform).Should().Equal(_appSettings.Platform);
        headers.GetValues(DeviceSessionHeaders.AppVersion).Should().Equal(_appSettings.AppVersion);
    }

    [Fact]
    public async Task SendAsync_Should_SendNoDeviceHeaders_When_DeviceIdIsMissing()
    {
        var inner = new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK));

        await SendAsync(inner);

        inner.Request!.Headers.Contains(DeviceSessionHeaders.DeviceId).Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_RaiseDeviceSessionRevoked_When_ApiReportsRevokedSession()
    {
        _appSettings.DeviceSettings.DeviceId = Guid.NewGuid();
        var raised = false;
        _sessionContext.DeviceSessionRevoked += (_, _) => raised = true;

        var response = await SendAsync(new RecordingHandler(Unauthorized(DeviceSessionErrorCodes.Revoked)));

        raised.Should().BeTrue();
        (await response.Content.ReadFromJsonAsync<ApiProblemDetails>(CurrentCancellationToken))!.ErrorCode
            .Should().Be(DeviceSessionErrorCodes.Revoked);
    }

    [Fact]
    public async Task SendAsync_Should_NotRaiseDeviceSessionRevoked_When_UnauthorizedForOtherReason()
    {
        _appSettings.DeviceSettings.DeviceId = Guid.NewGuid();
        var raised = false;
        _sessionContext.DeviceSessionRevoked += (_, _) => raised = true;

        await SendAsync(new RecordingHandler(Unauthorized("Authorization.Unauthorized")));

        raised.Should().BeFalse();
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMessageHandler inner)
    {
        using var invoker = new HttpMessageInvoker(new DeviceSessionDelegatingHandler(_appSettings, _sessionContext)
        {
            InnerHandler = inner
        });
        return await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "https://api.test/api/users/me"),
            CurrentCancellationToken);
    }

    private static HttpResponseMessage Unauthorized(string errorCode) =>
        ApiResponses.Problem(
            HttpStatusCode.Unauthorized,
            new ApiProblemDetails { Status = 401, ErrorCode = errorCode });

    private sealed class RecordingHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(response);
        }
    }
}