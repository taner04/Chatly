using System.Collections.Concurrent;
using System.Text.Json;
using Chatly.Desktop.Abstraction.Calls;
using Chatly.Desktop.Models.Calls;
using Chatly.Desktop.Utilities;
using Chatly.Desktop.Views.Calls;

namespace Chatly.Desktop.Services.Calls.Media;

[SingletonService(typeof(ICallMediaHost))]
internal sealed partial class WebViewCallMediaHost : ICallMediaHost, IDisposable
{
    private static readonly TimeSpan PageLoadTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(60);
    private static readonly JsonSerializerOptions ScriptJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ILogger<WebViewCallMediaHost> _logger;
    private readonly IMicrophoneAccessGranter _microphoneAccessGranter;
    private readonly SemaphoreSlim _pageLock = new(1, 1);
    private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> _pendingCommands = new();
    private readonly CallMediaView _view;
    private long _nextCommandId;
    private TaskCompletionSource? _pageLoaded;
    private CallMediaPageServer? _server;

    public WebViewCallMediaHost(
        CallMediaView view,
        IMicrophoneAccessGranter microphoneAccessGranter,
        ILogger<WebViewCallMediaHost> logger)
    {
        _view = view;
        _microphoneAccessGranter = microphoneAccessGranter;
        _logger = logger;
        _view.WebView.WebMessageReceived += OnWebMessageReceived;
        _view.WebView.NavigationCompleted += OnNavigationCompleted;
        _view.WebView.AdapterCreated += OnAdapterCreated;
    }

    public event EventHandler? RemoteParticipantJoined;

    public event EventHandler? RemoteParticipantLeft;

    public event EventHandler<string>? Disconnected;

    public event EventHandler<bool>? ReconnectingChanged;

    public Task JoinAsync(
        string serverUrl,
        string token,
        CallAudioOptions audioOptions,
        CancellationToken cancellationToken) =>
        RunCommandAsync("join", [serverUrl, token, audioOptions], cancellationToken);

    public Task ApplyAudioOptionsAsync(CallAudioOptions audioOptions) =>
        RunCommandAsync("applyAudio", [audioOptions], CancellationToken.None);

    public async Task<IReadOnlyList<CallAudioDevice>> GetAudioDevicesAsync(CancellationToken cancellationToken)
    {
        var result = await RunCommandAsync("listDevices", [], cancellationToken);
        return result.ValueKind != JsonValueKind.Array
            ? []
            : result.EnumerateArray()
                .Select(device => new CallAudioDevice(
                    device.GetProperty("id").GetString(),
                    device.GetProperty("label").GetString() ?? string.Empty,
                    device.GetProperty("kind").GetString() == "output"
                        ? CallAudioDeviceKind.Output
                        : CallAudioDeviceKind.Input))
                .ToList();
    }

    public Task SetMicrophoneEnabledAsync(bool enabled) =>
        RunCommandAsync("setMicrophone", [enabled], CancellationToken.None);

    public Task LeaveAsync() =>
        Volatile.Read(ref _pageLoaded) is null
            ? Task.CompletedTask
            : RunCommandAsync("leave", [], CancellationToken.None);

    public void Dispose()
    {
        _view.WebView.WebMessageReceived -= OnWebMessageReceived;
        _view.WebView.NavigationCompleted -= OnNavigationCompleted;
        _view.WebView.AdapterCreated -= OnAdapterCreated;
        _server?.Dispose();
        _pageLock.Dispose();
    }

    private async Task<JsonElement> RunCommandAsync(
        string command,
        object[] arguments,
        CancellationToken cancellationToken)
    {
        await EnsurePageLoadedAsync(cancellationToken);

        var id = Interlocked.Increment(ref _nextCommandId);
        var completion = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingCommands[id] = completion;
        try
        {
            var script =
                $"chatlyMedia.run({id}, {JsonSerializer.Serialize(command)}, {JsonSerializer.Serialize(arguments, ScriptJsonOptions)}), 1";
            await UiThreadDispatcher.SafeInvokeAsync(async () => await _view.WebView.InvokeScript(script));
            return await completion.Task.WaitAsync(CommandTimeout, cancellationToken);
        }
        finally
        {
            _pendingCommands.TryRemove(id, out _);
        }
    }

    private async Task EnsurePageLoadedAsync(CancellationToken cancellationToken)
    {
        TaskCompletionSource pageLoaded;
        await _pageLock.WaitAsync(cancellationToken);
        try
        {
            if (_pageLoaded is null)
            {
                _server = CallMediaPageServer.Start();
                _pageLoaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var address = _server.Address;
                UiThreadDispatcher.SafeInvoke(() =>
                {
                    GrantMicrophoneAccess(address);
                    _view.WebView.Source = address;
                });
            }

            pageLoaded = _pageLoaded;
        }
        finally
        {
            _pageLock.Release();
        }

        try
        {
            await pageLoaded.Task.WaitAsync(PageLoadTimeout, cancellationToken);
        }
        catch (TimeoutException)
        {
            await ResetPageAsync(pageLoaded);
            throw;
        }
    }

    private async Task ResetPageAsync(TaskCompletionSource failedPage)
    {
        await _pageLock.WaitAsync();
        try
        {
            if (!ReferenceEquals(_pageLoaded, failedPage))
            {
                return;
            }

            _pageLoaded = null;
            _server?.Dispose();
            _server = null;
        }
        finally
        {
            _pageLock.Release();
        }
    }

    private void GrantMicrophoneAccess(Uri pageAddress)
    {
        try
        {
            var handle = _view.WebView.TryGetPlatformHandle()
                         ?? throw new InvalidOperationException("The call media web view has no native handle yet.");
            _microphoneAccessGranter.GrantAccess(handle, pageAddress);
        }
        catch (Exception exception)
        {
            LogMicrophoneGrantFailed(exception);
        }
    }

    private async void OnAdapterCreated(object? sender, WebViewAdapterEventArgs e)
    {
        try
        {
            await EnsurePageLoadedAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            LogPagePreloadFailed(exception);
        }
    }

    private void OnNavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs e) =>
        Volatile.Read(ref _pageLoaded)?.TrySetResult();

    private void OnWebMessageReceived(object? sender, WebMessageReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Body))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(e.Body);
            var message = document.RootElement;
            switch (message.GetProperty("ev").GetString())
            {
                case "result":
                    CompleteCommand(message);
                    break;
                case "participantConnected":
                    RemoteParticipantJoined?.Invoke(this, EventArgs.Empty);
                    break;
                case "participantDisconnected":
                    RemoteParticipantLeft?.Invoke(this, EventArgs.Empty);
                    break;
                case "reconnecting":
                    ReconnectingChanged?.Invoke(this, true);
                    break;
                case "reconnected":
                    ReconnectingChanged?.Invoke(this, false);
                    break;
                case "disconnected":
                    var reason = message.GetProperty("reason").GetString() ?? "unknown";
                    LogMediaDisconnected(reason);
                    Disconnected?.Invoke(this, reason);
                    break;
            }
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException
                                              or InvalidOperationException)
        {
            LogInvalidMessage(exception);
        }
    }

    private void CompleteCommand(JsonElement message)
    {
        var id = message.GetProperty("id").GetInt64();
        if (!_pendingCommands.TryGetValue(id, out var completion))
        {
            return;
        }

        if (message.GetProperty("ok").GetBoolean())
        {
            completion.TrySetResult(
                message.TryGetProperty("value", out var value) ? value.Clone() : default);
            return;
        }

        var error = message.GetProperty("error").GetString() ?? "unknown error";
        completion.TrySetException(new InvalidOperationException($"Call media command failed: {error}"));
    }

    [LoggerMessage(LogLevel.Warning, "Call media disconnected: {Reason}.")]
    private partial void LogMediaDisconnected(string reason);

    [LoggerMessage(LogLevel.Warning, "Automatic microphone permission for call media could not be installed.")]
    private partial void LogMicrophoneGrantFailed(Exception exception);

    [LoggerMessage(LogLevel.Warning, "Call media page could not be preloaded.")]
    private partial void LogPagePreloadFailed(Exception exception);

    [LoggerMessage(LogLevel.Warning, "Call media page sent an invalid message.")]
    private partial void LogInvalidMessage(Exception exception);
}