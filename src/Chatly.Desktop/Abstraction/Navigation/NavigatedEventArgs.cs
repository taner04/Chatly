namespace Chatly.Desktop.Abstraction.Navigation;

public sealed class NavigatedEventArgs(Type viewModelType) : EventArgs
{
    public Type ViewModelType { get; } = viewModelType;
}