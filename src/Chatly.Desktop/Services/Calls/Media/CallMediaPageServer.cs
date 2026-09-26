using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Avalonia.Platform;

namespace Chatly.Desktop.Services.Calls.Media;

internal sealed class CallMediaPageServer : IDisposable
{
    private const string PagePath = "/";
    private const string ScriptPath = "/livekit-client.umd.js";

    private static readonly Uri PageAsset = new("avares://Chatly.Desktop/Assets/CallMedia/call-media.html");
    private static readonly Uri ScriptAsset = new("avares://Chatly.Desktop/Assets/CallMedia/livekit-client.umd.js");

    private readonly CancellationTokenSource _cancellation = new();
    private readonly TcpListener _listener;
    private readonly Dictionary<string, byte[]> _responses;

    private CallMediaPageServer(TcpListener listener, Dictionary<string, byte[]> responses)
    {
        _listener = listener;
        _responses = responses;
        Address = new Uri($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}{PagePath}");
    }

    internal Uri Address { get; }

    internal static CallMediaPageServer Start()
    {
        var responses = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [PagePath] = CreateResponse(PageAsset, "text/html; charset=utf-8"),
            [ScriptPath] = CreateResponse(ScriptAsset, "text/javascript; charset=utf-8")
        };

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var server = new CallMediaPageServer(listener, responses);
        _ = server.ServeAsync();
        return server;
    }

    public void Dispose()
    {
        _cancellation.Cancel();
        _listener.Stop();
        _cancellation.Dispose();
    }

    private static byte[] CreateResponse(Uri asset, string contentType)
    {
        using var stream = AssetLoader.Open(asset);
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        var body = memory.ToArray();
        var header = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 200 OK\r\nContent-Type: {contentType}\r\nContent-Length: {body.Length}\r\nCache-Control: no-store\r\nConnection: close\r\n\r\n");
        return [.. header, .. body];
    }

    private async Task ServeAsync()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync(_cancellation.Token);
            }
            catch (Exception) when (_cancellation.IsCancellationRequested)
            {
                return;
            }

            _ = RespondAsync(client);
        }
    }

    private async Task RespondAsync(TcpClient client)
    {
        using (client)
        {
            try
            {
                var stream = client.GetStream();
                var buffer = new byte[4096];
                var read = await stream.ReadAsync(buffer, _cancellation.Token);
                var requestLine = Encoding.ASCII.GetString(buffer, 0, read).Split("\r\n", 2)[0].Split(' ');
                var path = requestLine.Length > 1 ? requestLine[1] : string.Empty;
                var response = _responses.TryGetValue(path, out var found)
                    ? found
                    : [.. "HTTP/1.1 404 Not Found\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"u8];
                await stream.WriteAsync(response, _cancellation.Token);
            }
            catch (Exception) when (_cancellation.IsCancellationRequested)
            {
            }
            catch (IOException)
            {
            }
        }
    }
}
