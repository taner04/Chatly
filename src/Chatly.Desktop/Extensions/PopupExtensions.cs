using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.Extensions;

internal static class PopupExtensions
{
    extension(IPopupService popupService)
    {
        public Task ShowAsync<T>(CancellationToken cancellationToken = default)
            where T : IPopupViewModel =>
            popupService.ShowAsync(typeof(T), cancellationToken);

        public async Task<bool> ConfirmAsync(
            string title,
            string message,
            string confirmText,
            CancellationToken cancellationToken = default)
        {
            var popup = new ConfirmationPopupViewModel(title, message, confirmText, "Cancel");
            await popupService.ShowAsync(popup, cancellationToken);
            return popup.IsConfirmed;
        }

        public Task ShowMessageAsync(string title, string message, CancellationToken cancellationToken = default) =>
            popupService.ShowAsync(new ConfirmationPopupViewModel(title, message, "OK", null), cancellationToken);
    }
}