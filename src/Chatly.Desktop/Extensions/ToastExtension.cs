using Chatly.Desktop.Abstractions.Toasts;
using Chatly.Desktop.Services.Toasts;
using Chatly.Desktop.ViewModels.Toasts;

namespace Chatly.Desktop.Extentions;

public static class ToastExtension
{
    extension(IToastService toastService)
    {
        public void AddNotification(string message)
        {
            toastService.AddToast(BuildNotificiationViewModel("Notification", message, FluentIcons.Common.Symbol.Info, ToastType.Information));
        }

        public void ShowSuccess(string message)
        {
            toastService.AddToast(BuildNotificiationViewModel("Success", message, FluentIcons.Common.Symbol.CheckmarkCircle, ToastType.Success));
        }

        public void ShowError(string message)
        {
            toastService.AddToast(BuildNotificiationViewModel("Error", message, FluentIcons.Common.Symbol.ErrorCircle, ToastType.Error));
        }
    }

    private static ToastNotificationViewModel BuildNotificiationViewModel(
        string title,
        string message,
        FluentIcons.Common.Symbol icon,
        ToastType type)
    {
        return new ToastNotificationViewModel(title, message, icon, type);
    }
}
