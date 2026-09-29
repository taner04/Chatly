using Avalonia.Layout;

namespace Chatly.Desktop.Views.Calls;

[SingletonService]
internal sealed class CallMediaView : Decorator
{
    public CallMediaView()
    {
        Width = 1;
        Height = 1;
        IsHitTestVisible = false;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Bottom;
        Child = WebView;
    }

    internal NativeWebView WebView { get; } = new();
}