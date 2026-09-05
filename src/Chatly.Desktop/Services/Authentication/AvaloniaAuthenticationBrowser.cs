using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Avalonia.Platform;
using Duende.IdentityModel.OidcClient.Browser;

namespace Chatly.Desktop.Services.Authentication;

[SingletonService(typeof(IBrowser))]
public sealed class AvaloniaAuthenticationBrowser : IBrowser
{
    private static readonly Uri LoginCompletePageUri =
        new("avares://Chatly.Desktop/Assets/login-complete.html");

    private static readonly Uri LogoutCompletePageUri =
        new("avares://Chatly.Desktop/Assets/logout-complete.html");

    private static readonly Uri LogoUri =
        new("avares://Chatly.Desktop/Assets/chatly-logo.ico");

    public async Task<BrowserResult> InvokeAsync(
        BrowserOptions options,
        CancellationToken cancellationToken = default)
    {
        TcpListener? listener = null;

        try
        {
            var (redirectUri, address) = GetLoopbackRedirect(options.EndUrl);

            listener = new TcpListener(address, redirectUri.Port);
            listener.Start();

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

            if (!callbackUri.Scheme.Equals(redirectUri.Scheme, StringComparison.Ordinal) ||
                !callbackUri.Host.Equals(redirectUri.Host, StringComparison.OrdinalIgnoreCase) ||
                callbackUri.Port != redirectUri.Port ||
                !callbackUri.AbsolutePath.Equals(redirectUri.AbsolutePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Received an unexpected loopback request.");
            }

            await SendCompletionPageAsync(stream, IsLogout(options.StartUrl), cancellationToken);

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
            listener?.Stop();
        }
    }

    private static (Uri RedirectUri, IPAddress Address) GetLoopbackRedirect(string endUrl)
    {
        if (!Uri.TryCreate(endUrl, UriKind.Absolute, out var redirectUri) ||
            redirectUri.Scheme != Uri.UriSchemeHttp)
        {
            throw new InvalidOperationException(
                "The desktop redirect URI must use an HTTP loopback address.");
        }

        if (redirectUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return (redirectUri, IPAddress.Loopback);
        }

        if (!IPAddress.TryParse(redirectUri.Host, out var address) || !IPAddress.IsLoopback(address))
        {
            throw new InvalidOperationException(
                "The desktop redirect URI must use an HTTP loopback address.");
        }

        return (redirectUri, address);
    }

    private static string GetRequestTarget(string? requestLine)
    {
        var parts = requestLine?.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);

        if (parts is not ["GET", _, _])
        {
            throw new InvalidOperationException("Received an invalid loopback request.");
        }

        var requestTarget = parts[1];
        if (!requestTarget.StartsWith('/') || requestTarget.StartsWith("//", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Received an invalid loopback request target.");
        }

        return requestTarget;
    }

    private static async Task SendCompletionPageAsync(
        Stream stream,
        bool isLogout,
        CancellationToken cancellationToken)
    {
        var html = await RenderCompletionPageAsync(isLogout, cancellationToken);
        var body = Encoding.UTF8.GetBytes(html);
        var headers = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {body.Length}\r\nCache-Control: no-store\r\nContent-Security-Policy: default-src 'none'; img-src data:; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none'; frame-ancestors 'none'\r\nReferrer-Policy: no-referrer\r\nX-Content-Type-Options: nosniff\r\nConnection: close\r\n\r\n");

        await stream.WriteAsync(headers, cancellationToken);
        await stream.WriteAsync(body, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    private static async Task<string> RenderCompletionPageAsync(
        bool isLogout,
        CancellationToken cancellationToken)
    {
        using var htmlStream = AssetLoader.Open(isLogout ? LogoutCompletePageUri : LoginCompletePageUri);
        using var htmlReader = new StreamReader(htmlStream, Encoding.UTF8);
        var html = await htmlReader.ReadToEndAsync(cancellationToken);

        using var logoStream = AssetLoader.Open(LogoUri);
        using var logoBuffer = new MemoryStream();
        await logoStream.CopyToAsync(logoBuffer, cancellationToken);
        var logoDataUri = $"data:image/x-icon;base64,{Convert.ToBase64String(logoBuffer.ToArray())}";
        return html.Replace("{{CHATLY_LOGO_DATA_URI}}", logoDataUri, StringComparison.Ordinal);
    }

    private static bool IsLogout(string startUrl)
    {
        return Uri.TryCreate(startUrl, UriKind.Absolute, out var uri) &&
               (uri.AbsolutePath.Contains("logout", StringComparison.OrdinalIgnoreCase) ||
                uri.Query.Contains("post_logout_redirect_uri", StringComparison.OrdinalIgnoreCase));
    }
}