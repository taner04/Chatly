namespace Chatly.Desktop.Abstractions.Popups;

public interface IPopupOverlay<out T> where T : IPopupViewModel
{
    T ViewModel { get; }
}
