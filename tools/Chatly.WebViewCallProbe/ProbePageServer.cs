using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chatly.WebViewCallProbe;

internal sealed class ProbePageServer : IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly TcpListener _listener;
    private readonly byte[] _response;

    private ProbePageServer(TcpListener listener, byte[] page)
    {
        _listener = listener;
        var header = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {page.Length}\r\nConnection: close\r\n\r\n");
        _response = [.. header, .. page];
        Address = new Uri($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/");
    }

    internal Uri Address { get; }

    public void Dispose()
    {
        _cancellation.Cancel();
        _listener.Stop();
        _cancellation.Dispose();
    }

    internal static ProbePageServer Start()
    {
        using var stream = typeof(ProbePageServer).Assembly.GetManifestResourceStream("probe.html")
                           ?? throw new InvalidOperationException("The probe page is missing.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var server = new ProbePageServer(listener, memory.ToArray());
        _ = server.ServeAsync();
        return server;
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
                _ = await stream.ReadAsync(buffer, _cancellation.Token);
                await stream.WriteAsync(_response, _cancellation.Token);
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