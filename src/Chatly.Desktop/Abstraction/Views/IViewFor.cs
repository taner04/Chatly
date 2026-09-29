namespace Chatly.Desktop.Abstraction.Views;

public interface IViewFor<out TViewModel>
{
    TViewModel ViewModel { get; }
}