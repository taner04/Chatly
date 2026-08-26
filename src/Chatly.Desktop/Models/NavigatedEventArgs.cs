using System;
namespace Chatly.Desktop.Models;

public sealed class NavigatedEventArgs(Type viewModelType) : EventArgs
{
    public Type ViewModelType { get; } = viewModelType;
}
