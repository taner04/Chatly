using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace Chatly.WebViewCallProbe;

internal sealed class ProbeApplication(Uri pageAddress, string? reportPath) : Application
{
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan PhaseDuration = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan SettleDuration = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan MicrophoneTimeout = TimeSpan.FromSeconds(30);

    private readonly TaskCompletionSource _connected = new();
    private readonly TaskCompletionSource<JsonElement> _microphone = new();
    private readonly ProbeReport _report = new();
    private readonly DateTime _startedAt = DateTime.UtcNow;
    private ProbeTick? _lastTick;

    public override void Initialize() => Styles.Add(new FluentTheme());

    public override void OnFrameworkInitializationCompleted()
    {
        var desktop = (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!;
        var webView = new NativeWebView();
        webView.WebMessageReceived += (_, e) => OnMessage(e.Body);
        webView.AdapterDestroyed += (_, _) => Log("WebView adapter destroyed");

        var window = new Window
        {
            Title = "Chatly WebView call probe",
            Width = 480,
            Height = 320,
            Content = webView
        };
        desktop.MainWindow = window;
        window.Show();
        base.OnFrameworkInitializationCompleted();

        _ = RunAsync(desktop, window, webView);
    }

    private async Task RunAsync(
        IClassicDesktopStyleApplicationLifetime desktop,
        Window window,
        NativeWebView webView)
    {
        Log($"OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
        Log($"Page: {pageAddress}");
        webView.Source = pageAddress;

        var connected = await Task.WhenAny(_connected.Task, Task.Delay(ConnectTimeout)) == _connected.Task;
        _report.Add("WebRTC loopback connects", connected, connected ? "connected" : $"no connection within {ConnectTimeout.TotalSeconds:F0}s");

        if (connected)
        {
            await MeasureFlowAsync("Audio flows while visible", () => { });
            await MeasureFlowAsync("Audio flows while WebView is hidden", () => webView.IsVisible = false);
            webView.IsVisible = true;
            await Task.Delay(SettleDuration);
            await MeasureFlowAsync("Audio flows while window is minimized", () => window.WindowState = WindowState.Minimized);
            window.WindowState = WindowState.Normal;
            await Task.Delay(SettleDuration);
        }

        await CheckMicrophoneAsync(webView);

        var exitCode = _report.Print();
        if (reportPath is not null)
        {
            _report.Write(reportPath);
        }

        desktop.Shutdown(exitCode);
    }

    private async Task MeasureFlowAsync(string name, Action enterPhase)
    {
        Log($"Phase: {name}");
        enterPhase();
        await Task.Delay(SettleDuration);
        var start = _lastTick;
        await Task.Delay(PhaseDuration);
        var end = _lastTick;

        if (start is null || end is null)
        {
            _report.Add(name, false, "no statistics received");
            return;
        }

        var elapsedSeconds = (end.Time - start.Time) / 1000;
        var packetsPerSecond = elapsedSeconds > 0 ? (end.Received - start.Received) / elapsedSeconds : 0;
        var passed = packetsPerSecond >= 40 && end.Rms > 0.1;
        _report.Add(
            name,
            passed,
            $"{packetsPerSecond:F0} packets/s, lost {end.Lost - start.Lost}, concealed {end.Concealed - start.Concealed} samples, level {end.Rms:F2}, context {end.AudioContext}");
    }

    private async Task CheckMicrophoneAsync(NativeWebView webView)
    {
        Log("Phase: microphone (allow access if a prompt appears)");
        await webView.InvokeScript("requestMicrophone(), 1");
        var completed = await Task.WhenAny(_microphone.Task, Task.Delay(MicrophoneTimeout)) == _microphone.Task;
        if (!completed)
        {
            _report.Add("Microphone access", false, $"no answer within {MicrophoneTimeout.TotalSeconds:F0}s");
            _report.Add("Echo cancellation enabled", false, "microphone unavailable");
            return;
        }

        var result = _microphone.Task.Result;
        if (!result.GetProperty("ok").GetBoolean())
        {
            _report.Add("Microphone access", false, result.GetProperty("error").GetString() ?? "denied");
            _report.Add("Echo cancellation enabled", false, "microphone unavailable");
            return;
        }

        var settings = result.GetProperty("settings");
        _report.Add("Microphone access", true, result.GetProperty("label").GetString() ?? "granted");
        var echoCancellation = settings.TryGetProperty("echoCancellation", out var value) && value.ValueKind == JsonValueKind.True;
        _report.Add("Echo cancellation enabled", echoCancellation, settings.ToString());
    }

    private void OnMessage(string? body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return;
        }

        using var document = JsonDocument.Parse(body);
        var message = document.RootElement.Clone();
        switch (message.GetProperty("ev").GetString())
        {
            case "tick":
                _lastTick = ProbeTick.From(message);
                Log($"tick {_lastTick}");
                break;
            case "connection":
                Log(body);
                if (message.GetProperty("state").GetString() == "connected")
                {
                    _connected.TrySetResult();
                }

                break;
            case "microphone":
                Log(body);
                _microphone.TrySetResult(message);
                break;
            case "env":
                Log(body);
                _report.Add("Secure context with RTCPeerConnection",
                    message.GetProperty("secure").GetBoolean() && message.GetProperty("rtc").GetBoolean(),
                    message.GetProperty("userAgent").GetString() ?? "");
                break;
            case "playback":
                Log(body);
                _report.Add("Remote audio plays without user gesture",
                    message.GetProperty("ok").GetBoolean(),
                    message.TryGetProperty("error", out var error) ? error.GetString() ?? "" : "play() resolved");
                break;
            case "codec":
                Log(body);
                var codec = message.GetProperty("codec").GetString();
                _report.Add("Opus offered", codec is not null, codec ?? "no opus in SDP");
                break;
            default:
                Log(body);
                break;
        }
    }

    private void Log(string message) =>
        Console.WriteLine($"[{(DateTime.UtcNow - _startedAt).TotalSeconds,5:F1}s] {message}");
}
