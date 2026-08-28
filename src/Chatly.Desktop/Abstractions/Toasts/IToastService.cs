using System;

namespace Chatly.Desktop.Abstractions.Toasts;

public interface IToastService
{
    void SetToastHost(IToastHost toastHost);

    void AddToast(IToastViewModel toastViewModel);
    void RemoveToast(Guid id);
}
