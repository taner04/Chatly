namespace Chatly.Desktop.Abstraction.Navigation;

public interface INavigableView<out T> where T : INavigableViewModel
{
    T ViewModel { get; }
}