using System;

namespace Chatly.Desktop.Models;

public sealed class NavigatedEventArgs(Type pageType, object? parameter) : EventArgs
{
    public Type PageType { get; } = pageType;

    public object? Parameter { get; } = parameter;
}
