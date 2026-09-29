using Chatly.WebViewCallProbe.IntegrationTests.Infrastructure;

namespace Chatly.WebViewCallProbe.IntegrationTests.Tests;

public sealed class WebViewCallProbeTests(ProbeRunFixture probe) : IClassFixture<ProbeRunFixture>
{
    public static TheoryData<string> Checks =>
    [
        "Secure context with RTCPeerConnection",
        "Remote audio plays without user gesture",
        "Opus offered",
        "WebRTC loopback connects",
        "Audio flows while visible",
        "Audio flows while WebView is hidden",
        "Audio flows while window is minimized",
        "Microphone access",
        "Echo cancellation enabled"
    ];

    [Theory(Explicit = true)]
    [MemberData(nameof(Checks))]
    public async Task Check_Should_Pass_When_ProbeRunsOnThisMachine(string name)
    {
        var checks = await probe.GetChecksAsync();
        var check = checks.SingleOrDefault(candidate => candidate.Name == name);

        Assert.True(check is not null, $"'{name}' was not reported by the probe.{Environment.NewLine}{probe.Output}");
        Assert.True(check.Passed, $"{name}: {check.Detail}");
    }

    [Fact(Explicit = true)]
    public async Task Report_Should_ContainOnlyKnownChecks_When_ProbeCompletes()
    {
        var known = Checks.Select(row => row.Data).ToHashSet();

        var checks = await probe.GetChecksAsync();

        Assert.All(checks, check => Assert.Contains(check.Name, known));
    }
}