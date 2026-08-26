namespace Chatly.Desktop.Abstractions.Navigation;

public interface INavigableView<out T> where T : INavigableViewModel
{
    T ViewModel { get; }
}
