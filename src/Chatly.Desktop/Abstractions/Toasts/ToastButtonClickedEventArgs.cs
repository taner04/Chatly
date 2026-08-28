using System;
using Chatly.Desktop.Services.Toasts;

namespace Chatly.Desktop.Abstractions.Toasts;

public sealed class ToastButtonClickedEventArgs(
    ToastButton? button,
    bool isDismissed = false) : EventArgs
{
    public ToastButton? Button { get; } = button;
    public bool IsDismissed { get; } = isDismissed;
}
