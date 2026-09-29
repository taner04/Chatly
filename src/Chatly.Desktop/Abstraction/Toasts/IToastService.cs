using Chatly.Desktop.ViewModels.Toasts;

namespace Chatly.Desktop.Abstraction.Toasts;

public interface IToastService
{
    void AddToast(ToastViewModel toastViewModel);
}