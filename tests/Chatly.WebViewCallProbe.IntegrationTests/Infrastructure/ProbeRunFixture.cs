using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Chatly.WebViewCallProbe.IntegrationTests.Infrastructure;

public sealed class ProbeRunFixture
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromMinutes(3);

    private readonly Lazy<Task<IReadOnlyList<ProbeCheck>>> _checks;
    private readonly StringBuilder _output = new();

    public ProbeRunFixture()
    {
        _checks = new Lazy<Task<IReadOnlyList<ProbeCheck>>>(RunAsync);
    }

    public string Output => _output.ToString();

    public string ReportPath { get; } = Path.Combine(
        AppContext.BaseDirectory,
        "TestResults",
        $"webview-call-probe-{OperatingSystemName()}.json");

    public Task<IReadOnlyList<ProbeCheck>> GetChecksAsync() => _checks.Value;

    private async Task<IReadOnlyList<ProbeCheck>> RunAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath)!);
        File.Delete(ReportPath);

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo("dotnet")
        {
            ArgumentList = { ProbeAssemblyPath(), "--report", ReportPath },
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        process.OutputDataReceived += (_, e) => AppendLine(e.Data);
        process.ErrorDataReceived += (_, e) => AppendLine(e.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeout = new CancellationTokenSource(ProbeTimeout);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(true);
            throw new TimeoutException($"The probe did not finish within {ProbeTimeout}.{Environment.NewLine}{Output}");
        }

        if (!File.Exists(ReportPath))
        {
            throw new InvalidOperationException(
                $"The probe exited with code {process.ExitCode} without writing {ReportPath}.{Environment.NewLine}{Output}");
        }

        await using var report = File.OpenRead(ReportPath);
        return await JsonSerializer.DeserializeAsync<List<ProbeCheck>>(
                   report,
                   new JsonSerializerOptions(JsonSerializerDefaults.Web))
               ?? [];
    }

    private void AppendLine(string? line)
    {
        if (line is null)
        {
            return;
        }

        lock (_output)
        {
            _output.AppendLine(line);
        }
    }

    private static string ProbeAssemblyPath() =>
        typeof(ProbeRunFixture).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "ProbeAssemblyPath")
            .Value!;

    private static string OperatingSystemName() =>
        OperatingSystem.IsMacOS() ? "macos" : OperatingSystem.IsWindows() ? "windows" : "linux";
}