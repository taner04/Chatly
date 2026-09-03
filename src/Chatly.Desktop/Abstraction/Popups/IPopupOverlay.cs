using Chatly.Desktop.Services.Popups;

namespace Chatly.Desktop.Abstraction.Popups;

public interface IPopupOverlay<out T> where T : IPopupViewModel
{
    T ViewModel { get; }

    public virtual void HandlePopupEvent(object? sender, PopupOverlayEventArgs args)
    {
    }
}