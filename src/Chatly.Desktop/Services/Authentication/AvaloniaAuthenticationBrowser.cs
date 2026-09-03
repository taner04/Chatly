using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Duende.IdentityModel.OidcClient.Browser;

namespace Chatly.Desktop.Services.Authentication;

public sealed class AvaloniaAuthenticationBrowser : IBrowser
{
    public async Task<BrowserResult> InvokeAsync(
        BrowserOptions options,
        CancellationToken cancellationToken = default)
    {
        var redirectUri = new Uri(options.EndUrl);

        if (redirectUri.Scheme != Uri.UriSchemeHttp ||
            !IPAddress.TryParse(redirectUri.Host, out var address) ||
            !IPAddress.IsLoopback(address))
        {
            throw new InvalidOperationException(
                "The desktop redirect URI must use an HTTP loopback address.");
        }

        var listener = new TcpListener(address, redirectUri.Port);
        listener.Start();

        try
        {
            Process.Start(new ProcessStartInfo(options.StartUrl)
            {
                UseShellExecute = true
            });

            using var client = await listener.AcceptTcpClientAsync(cancellationToken);
            await using var stream = client.GetStream();
            using var reader = new StreamReader(
                stream,
                Encoding.ASCII,
                false,
                leaveOpen: true);

            var requestTarget = GetRequestTarget(await reader.ReadLineAsync(cancellationToken));
            var callbackUri = new Uri(redirectUri, requestTarget);

            if (!callbackUri.AbsolutePath.Equals(
                    redirectUri.AbsolutePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Received an unexpected loopback request.");
            }

            await SendCompletionPageAsync(stream, cancellationToken);

            return new BrowserResult
            {
                ResultType = BrowserResultType.Success,
                Response = callbackUri.AbsoluteUri
            };
        }
        catch (OperationCanceledException)
        {
            return new BrowserResult { ResultType = BrowserResultType.UserCancel };
        }
        catch (Exception exception)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UnknownError,
                Error = exception.Message
            };
        }
        finally
        {
            listener.Stop();
        }
    }

    private static string GetRequestTarget(string? requestLine)
    {
        var parts = requestLine?.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);

        if (parts is not ["GET", _, _])
        {
            throw new InvalidOperationException("Received an invalid loopback request.");
        }

        return parts[1];
    }

    private static async Task SendCompletionPageAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        const string html = """
                            <!doctype html>
                            <html lang="en">
                            <head><meta charset="utf-8"><title>Chatly</title></head>
                            <body style="font-family:sans-serif;text-align:center;padding:4rem">
                              <h1>Sign-in complete</h1>
                              <p>You can close this tab and return to Chatly.</p>
                            </body>
                            </html>
                            """;

        var body = Encoding.UTF8.GetBytes(html);
        var headers = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");

        await stream.WriteAsync(headers, cancellationToken);
        await stream.WriteAsync(body, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }
}