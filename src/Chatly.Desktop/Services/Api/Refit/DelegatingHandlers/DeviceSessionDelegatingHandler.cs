using System.Net;
using System.Net.Http;
using System.Text.Json;
using Chatly.Desktop.Models.Settings;

namespace Chatly.Desktop.Services.Api.Refit.DelegatingHandlers;

[TransientService]
internal sealed class DeviceSessionDelegatingHandler(
    AppSettings appSettings,
    UserSessionContext sessionContext) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        foreach (var (name, value) in DeviceSessionHeaderValues.Create(appSettings))
        {
            request.Headers.Add(name, value);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized && await IsDeviceSessionRevokedAsync(response))
        {
            sessionContext.NotifyDeviceSessionRevoked();
        }

        return response;
    }

    private static async Task<bool> IsDeviceSessionRevokedAsync(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            var problem = JsonSerializer.Deserialize<ApiProblemDetails>(content, JsonSerializerOptions.Web);
            return problem is { ErrorCode: DeviceSessionErrorCodes.Revoked };
        }
        catch (JsonException)
        {
            return false;
        }
    }
}