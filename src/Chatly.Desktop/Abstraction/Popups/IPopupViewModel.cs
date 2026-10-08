namespace Chatly.Desktop.Abstraction.Popups;

public interface IPopupViewModel
{
    string Title { get; }

    bool IsDismissible { get; }

    double PopupWidth { get; }

    double PopupMinHeight { get; }

    Task Completion { get; }

    void CloseOverlay();
}