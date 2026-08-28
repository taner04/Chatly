using System;

namespace Chatly.Desktop.Services.Popups;

public sealed class PopupOverlayEventArgs(PopupOverlayHostEventType type, object data = null!) : EventArgs
{
    public PopupOverlayHostEventType Type { get; } = type;
    public object? Data { get; } = data;
}
