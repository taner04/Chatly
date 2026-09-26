using Chatly.Desktop.Services.Api.Results;
using Chatly.Desktop.Services.Toasts;
using Chatly.Desktop.ViewModels.Toasts;
using FluentIcons.Common;

namespace Chatly.Desktop.Extensions;

internal static class ToastExtensions
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
            toastService.ShowError(error.Detail);
        }

        public void ShowError(string message)
        {
            toastService.AddToast(BuildNotificationViewModel("Something went wrong", message, Symbol.ErrorCircle,
                ToastType.Error));
        }
    }

    private static ToastViewModel BuildNotificationViewModel(string title, string message, Symbol icon,
        ToastType type) =>
        new(title, message, icon, type);
}