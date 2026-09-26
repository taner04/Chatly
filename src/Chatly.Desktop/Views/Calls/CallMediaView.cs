using Avalonia.Controls.Primitives;

namespace Chatly.Desktop.Views.Calls;

[SingletonService]
internal sealed class CallMediaView : Decorator
{
    public CallMediaView()
    {
        Width = 1;
        Height = 1;
        IsHitTestVisible = false;
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
        Child = WebView;
    }

    internal NativeWebView WebView { get; } = new();
}
