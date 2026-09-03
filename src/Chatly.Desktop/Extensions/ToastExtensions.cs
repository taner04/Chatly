using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Services.Api.Results;
using Chatly.Desktop.Services.Toasts;
using Chatly.Desktop.ViewModels.Toasts;
using FluentIcons.Common;

namespace Chatly.Desktop.Extensions;

public static class ToastExtensions
{
    extension(IToastService toastService)
    {
        public void AddNotification(string message)
        {
            toastService.AddToast(BuildNotificationViewModel("Notification", message, Symbol.Info,
                ToastType.Information));
        }

        public void ShowSuccess(string message)
        {
            toastService.AddToast(BuildNotificationViewModel("Success", message, Symbol.CheckmarkCircle,
                ToastType.Success));
        }

        public void ShowError(WebClientError error)
        {
            toastService.AddToast(BuildNotificationViewModel(error.ErrorCode, error.Detail, Symbol.ErrorCircle,
                ToastType.Error));
        }
    }

    private static ToastNotificationViewModel BuildNotificationViewModel(
        string title,
        string message,
        Symbol icon,
        ToastType type)
    {
        return new ToastNotificationViewModel(title, message, icon, type);
    }
}