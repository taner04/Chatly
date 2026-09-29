namespace Chatly.Desktop.Abstraction.Navigation;

public sealed class NavigatedEventArgs(INavigableViewModel page) : EventArgs
{
    public INavigableViewModel Page { get; } = page;
}