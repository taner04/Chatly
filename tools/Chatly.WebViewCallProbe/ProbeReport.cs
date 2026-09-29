using System.Text.Json;

namespace Chatly.WebViewCallProbe;

internal sealed class ProbeReport
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        { WriteIndented = true };

    private readonly List<ProbeCheck> _results = [];

    internal void Add(string name, bool passed, string detail) => _results.Add(new ProbeCheck(name, passed, detail));

    internal int Print()
    {
        Console.WriteLine();
        Console.WriteLine("WebView call probe results");
        foreach (var (name, passed, detail) in _results)
        {
            Console.WriteLine($"  {(passed ? "PASS" : "FAIL")}  {name}: {detail}");
        }

        var failed = _results.Count(result => !result.Passed);
        Console.WriteLine(failed == 0 ? "All checks passed." : $"{failed} check(s) failed.");
        return failed == 0 ? 0 : 1;
    }

    internal void Write(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(_results, JsonOptions));
    }

    private sealed record ProbeCheck(string Name, bool Passed, string Detail);
}