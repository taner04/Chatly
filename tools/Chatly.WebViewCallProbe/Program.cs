using Avalonia;
using Chatly.WebViewCallProbe;

var reportIndex = Array.IndexOf(args, "--report");
var reportPath = reportIndex >= 0 && reportIndex + 1 < args.Length ? args[reportIndex + 1] : null;
var server = ProbePageServer.Start();
return AppBuilder.Configure(() => new ProbeApplication(server.Address, reportPath))
    .UsePlatformDetect()
    .StartWithClassicDesktopLifetime(args);
