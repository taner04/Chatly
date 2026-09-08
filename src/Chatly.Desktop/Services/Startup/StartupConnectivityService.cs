using System.Net.Http;
using Chatly.Desktop.Services.Api.Refit.Abstraction;

namespace Chatly.Desktop.Services.Startup;

[TransientService]
internal sealed class StartupConnectivityService(IChatlyApi chatlyApi)
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(3);

    public async Task<bool> IsReadyAsync(CancellationToken cancellationToken)
    {
        using var requestCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        requestCancellation.CancelAfter(RequestTimeout);

        try
        {
            using var response = await chatlyApi.CheckStatusAsync(requestCancellation.Token);
            return response.IsSuccessful;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}
