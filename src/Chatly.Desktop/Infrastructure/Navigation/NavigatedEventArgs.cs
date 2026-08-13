using System;

namespace Chatly.Desktop.Infrastructure.Navigation;

public sealed class NavigatedEventArgs(Type pageType, object? parameter) : EventArgs
{
    public Type PageType { get; } = pageType;

    public object? Parameter { get; } = parameter;
}
