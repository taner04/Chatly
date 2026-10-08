namespace Chatly.Desktop.ViewModels.Popups;

public sealed partial class ConfirmationPopupViewModel(
    string title,
    string message,
    string confirmText,
    string? cancelText) : PopupOverlayViewModel
{
    public override string Title { get; } = title;

    public override bool IsDismissible => CancelText is not null;

    public override double PopupWidth => 420;

    public override double PopupMinHeight => 0;

    public string Message { get; } = message;

    public string ConfirmText { get; } = confirmText;

    public string? CancelText { get; } = cancelText;

    public bool IsCancelVisible => CancelText is not null;

    public bool IsConfirmed { get; private set; }

    [RelayCommand]
    private void Confirm()
    {
        IsConfirmed = true;
        CloseOverlay();
    }

    [RelayCommand]
    private void Cancel() => CloseOverlay();
}