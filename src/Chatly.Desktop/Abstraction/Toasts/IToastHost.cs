namespace Chatly.Desktop.Abstraction.Toasts;

public interface IToastHost
{
    void AddToast(IToastViewModel toastViewModel);
    void RemoveToast(Guid id);
}