using System.IO;
using System.Net.Http;

namespace Chatly.Desktop.Services.Storage;

[SingletonService]
public sealed class FileDownloadClient
{
    private static readonly HttpClient HttpClient = new();

    internal async Task DownloadAsync(
        string url,
        Stream destination,
        CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await response.Content.CopyToAsync(destination, cancellationToken);
    }
}