using System;

namespace Chatly.Desktop.Abstractions.Toasts;

public interface IToastHost
{
    void AddToast(IToastViewModel toastViewModel);
    void RemoveToast(Guid id);
}
