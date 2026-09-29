using System.Text.Json;

namespace Chatly.WebViewCallProbe;

internal sealed record ProbeTick(
    double Time,
    double Rms,
    long Received,
    long Lost,
    long Concealed,
    string AudioContext,
    string Visibility)
{
    internal static ProbeTick From(JsonElement message) => new(
        message.GetProperty("time").GetDouble(),
        message.GetProperty("rms").GetDouble(),
        message.GetProperty("received").GetInt64(),
        message.GetProperty("lost").GetInt64(),
        message.GetProperty("concealed").GetInt64(),
        message.GetProperty("audioContext").GetString() ?? "",
        message.GetProperty("visibility").GetString() ?? "");

    public override string ToString() =>
        $"received {Received}, lost {Lost}, concealed {Concealed}, level {Rms:F2}, context {AudioContext}, page {Visibility}";
}