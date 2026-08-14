namespace Chatly.Desktop.Abstractions.Overlays;

public interface IPopupOverlay;

public interface IPopupOverlay<out T> : IPopupOverlay where T : IPopupViewModel
{
    T ViewModel { get; }
}
